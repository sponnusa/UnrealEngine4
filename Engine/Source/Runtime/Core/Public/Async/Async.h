// Copyright 1998-2015 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "Future.h"
#include "TaskGraphInterfaces.h"


/**
 * Enumerates available asynchronous execution methods.
 */
enum class EAsyncExecution
{
	/** Execute in Task Graph (for short running tasks). */
	TaskGraph,

	/** Execute in separate thread (for long running tasks). */
	Thread,

	/** Execute in queued thread pool. */
	ThreadPool
};


/**
 * Template for setting a promise's value from a function.
 */
template<typename ResultType>
inline void SetPromise(TPromise<ResultType>& Promise, TFunction<ResultType()> Function)
{
	Promise.SetValue(Function());
}


/**
 * Template for setting a promise's value from a function (specialization for void results).
 */
template<>
inline void SetPromise(TPromise<void>& Promise, TFunction<void()> Function)
{
	Function();
	Promise.SetValue();
}


/**
 * Base class for asynchronous functions that are executed in the Task Graph system.
 */
class FAsyncGraphTaskBase
{
public:

	/**
	 * Returns the name of the thread that this task should run on.
	 *
	 * @return Always run on any thread.
	 */
	ENamedThreads::Type GetDesiredThread()
	{
		return ENamedThreads::AnyThread;
	}

	/**
	 * Gets the task's stats tracking identifier.
	 *
	 * @return Stats identifier.
	 */
	TStatId GetStatId() const
	{
		return GET_STATID(STAT_TaskGraph_OtherTasks);
	}

	/**
	 * Gets the mode for tracking subsequent tasks.
	 *
	 * @return Always track subsequent tasks.
	 */
	static ESubsequentsMode::Type GetSubsequentsMode()
	{
		return ESubsequentsMode::FireAndForget;
	}
};


/**
 * Template for asynchronous functions that are executed in the Task Graph system.
 */
template<typename ResultType>
class TAsyncGraphTask
	: public FAsyncGraphTaskBase
{
public:

	/**
	 * Creates and initializes a new instance.
	 *
	 * @param InFunction The function to execute asynchronously.
	 * @param InPromise The promise object used to return the function's result.
	 */
	TAsyncGraphTask(TFunction<ResultType()>&& InFunction, TPromise<ResultType>&& InPromise)
		: Function(MoveTemp(InFunction))
		, Promise(MoveTemp(InPromise))
	{ }

public:

	/**
	 * Performs the actual task.
	 *
	 * @param CurrentThread The thread that this task is executing on.
	 * @param MyCompletionGraphEvent The completion event.
	 */
	void DoTask(ENamedThreads::Type CurrentThread, const FGraphEventRef& MyCompletionGraphEvent)
	{
		SetPromise(Promise, Function);
	}

	/**
	 * Gets the future that will hold the asynchronous result.
	 *
	 * @return A TFuture object.
	 */
	TFuture<ResultType> GetFuture()
	{
		return Promise.GetFuture();
	}

private:

	/** The function to execute on the Task Graph. */
	TFunction<ResultType()> Function;

	/** The promise to assign the result to. */
	TPromise<ResultType> Promise;
};


/**
 * Template for asynchronous functions that are executed in a separate thread.
 */
template<typename ResultType>
class TAsyncRunnable
	: public FRunnable
{
public:

	/**
	 * Creates and initializes a new instance.
	 *
	 * @param InFunction The function to execute asynchronously.
	 * @param InPromise The promise object used to return the function's result.
	 * @param InThreadFuture The thread that is running this task.
	 */
	TAsyncRunnable(TFunction<ResultType()>&& InFunction, TPromise<ResultType>&& InPromise, TFuture<FRunnableThread*>&& InThreadFuture)
		: Function(InFunction)
		, Promise(MoveTemp(InPromise))
		, ThreadFuture(MoveTemp(InThreadFuture))
	{ }

public:

	// FRunnable interface

	virtual uint32 Run() override;

private:

	/** The function to execute on the Task Graph. */
	TFunction<ResultType()> Function;

	/** The promise to assign the result to. */
	TPromise<ResultType> Promise;

	/** The thread running this task. */
	TFuture<FRunnableThread*> ThreadFuture;
};


/**
 * Template for asynchronous functions that are executed in the queued thread pool.
 */
template<typename ResultType>
class TAsyncQueuedWork
	: public IQueuedWork
{
public:

	/**
	 * Creates and initializes a new instance.
	 *
	 * @param InFunction The function to execute asynchronously.
	 * @param InPromise The promise object used to return the function's result.
	 */
	TAsyncQueuedWork(TFunction<ResultType()>&& InFunction, TPromise<ResultType>&& InPromise)
		: Function(InFunction)
		, Promise(MoveTemp(InPromise))
	{ }

public:

	// IQueuedWork interface

	virtual void DoThreadedWork() override
	{
		SetPromise(Promise, Function);
		delete this;
	}

	virtual void Abandon() override
	{
		// not supported
	}

private:

	/** The function to execute on the Task Graph. */
	TFunction<ResultType()> Function;

	/** The promise to assign the result to. */
	TPromise<ResultType> Promise;
};


/**
 * Helper struct used to generate unique ids for the stats.
 */
struct FAsyncThreadIndex
{
	CORE_API static int32 GetNext()
	{
		static FThreadSafeCounter ThreadIndex;
		return ThreadIndex.Add(1);
	}
};


/**
 * Executes a given function asynchronously.
 *
 * While we still support VS2012, we need to help the compiler deduce the ResultType
 * template argument for Async<T>(), which can be accomplished in two ways:
 *
 *	- call Async() with the template parameter, i.e. Async<int>(..)
 *	- use a TFunction binding that explicitly specifies the type, i.e. TFunction<int()> F = []() { .. }
 *
 * Usage examples:
 *
 *	// using global function
 *		int TestFunc()
 *		{
 *			return 123;
 *		}
 *
 *		TFunction<int()> Task = TestFunc();
 *		auto Result = Async(EAsyncExecution::Thread, Task);
 *
 *	// using lambda
 *		TFunction<int()> Task = []()
 *		{
 *			return 123;
 *		}
 *
 *		auto Result = Async(EAsyncExecution::Thread, Task);
 *
 *
 *	// using inline lambda
 *		auto Result = Async<int>(EAsyncExecution::Thread, []() {
 *			return 123;
 *		}
 *
 * @param Execution The execution method to use, i.e. on Task Graph or in a separate thread.
 * @param Function The function to execute.
 * @result A TFuture object that will receive the return value from the function.
 */
template<typename ResultType>
TFuture<ResultType> Async(EAsyncExecution Execution, TFunction<ResultType()> Function)
{
	TPromise<ResultType> Promise;
	TFuture<ResultType> Future = Promise.GetFuture();

	switch (Execution)
	{
	case EAsyncExecution::TaskGraph:
		{
			TGraphTask<TAsyncGraphTask<ResultType>>::CreateTask().ConstructAndDispatchWhenReady(MoveTemp(Function), MoveTemp(Promise));
		}
		break;
	
	case EAsyncExecution::Thread:
		{
			TPromise<FRunnableThread*> ThreadPromise;
			TAsyncRunnable<ResultType>* Runnable = new TAsyncRunnable<ResultType>(MoveTemp(Function), MoveTemp(Promise), ThreadPromise.GetFuture());
			
			const FString TAsyncThreadName = FString::Printf(TEXT("TAsync %d"), FAsyncThreadIndex::GetNext());
			FRunnableThread* RunnableThread = FRunnableThread::Create(Runnable, *TAsyncThreadName);

			check(RunnableThread != nullptr);

			ThreadPromise.SetValue(RunnableThread);
		}
		break;

	case EAsyncExecution::ThreadPool:
		{
			GThreadPool->AddQueuedWork(new TAsyncQueuedWork<ResultType>(MoveTemp(Function), MoveTemp(Promise)));
		}
		break;

	default:
		check(false); // not implemented yet!
	}

	return MoveTemp(Future);
}


template<typename ResultType> uint32 TAsyncRunnable<ResultType>::Run()
{
	SetPromise(Promise, Function);

	FRunnableThread* Thread = ThreadFuture.Get();

	// Enqueue deletion of the thread to a different thread.
	Async<void>(EAsyncExecution::TaskGraph, [=]() {
			delete Thread;
			delete this;
		}
	);

	return 0;
}
