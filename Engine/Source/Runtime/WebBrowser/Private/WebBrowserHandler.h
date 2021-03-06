// Copyright 1998-2015 Epic Games, Inc. All Rights Reserved.

#pragma once

#if WITH_CEF3

#if PLATFORM_WINDOWS
	#include "AllowWindowsPlatformTypes.h"
#endif

#pragma push_macro("OVERRIDE")
#undef OVERRIDE // cef headers provide their own OVERRIDE macro
#include "include/cef_client.h"
#include "include/wrapper/cef_message_router.h"
#pragma pop_macro("OVERRIDE")

#if PLATFORM_WINDOWS
	#include "HideWindowsPlatformTypes.h"
#endif


class FWebBrowserWindow;


/**
 * Implements CEF Client and other Browser level interfaces.
 */
class FWebBrowserHandler
	: public CefClient
	, public CefDisplayHandler
	, public CefLifeSpanHandler
	, public CefLoadHandler
	, public CefRenderHandler
	, public CefRequestHandler
	, public CefKeyboardHandler
	, public CefMessageRouterBrowserSide::Handler
{
public:

	/** Default constructor. */
	FWebBrowserHandler();

public:

	/**
	 * Pass in a pointer to our Browser Window so that events can be passed on.
	 *
	 * @param InBrowserWindow The browser window this will be handling.
	 */
	void SetBrowserWindow(TSharedPtr<FWebBrowserWindow> InBrowserWindow);
	
	/**
	 * Pass in a pointer to our parent Browser Window.
	 *
	 * @param InBrowserWindow The parent browser window.
	 */
	void SetBrowserWindowParent(TSharedPtr<FWebBrowserWindow> InBrowserWindow);

	/** Sets whether to show messages on loading errors. */
	void SetShowErrorMessage(bool InShowErrorMessage)
	{
		ShowErrorMessage = InShowErrorMessage;
	}


public:

	// CefClient Interface

	virtual CefRefPtr<CefDisplayHandler> GetDisplayHandler() override
	{
		return this;
	}

	virtual CefRefPtr<CefLifeSpanHandler> GetLifeSpanHandler() override
	{
		return this;
	}

	virtual CefRefPtr<CefLoadHandler> GetLoadHandler() override
	{
		return this;
	}

	virtual CefRefPtr<CefRenderHandler> GetRenderHandler() override
	{
		return this;
	}

	virtual CefRefPtr<CefRequestHandler> GetRequestHandler() override
	{
		return this;
	}

	virtual CefRefPtr<CefKeyboardHandler> GetKeyboardHandler() override
	{
		return this;
	}

	virtual bool OnProcessMessageReceived(CefRefPtr<CefBrowser> Browser,
		CefProcessId SourceProcess,
		CefRefPtr<CefProcessMessage> Message) override;


public:

	// CefDisplayHandler Interface

	virtual void OnTitleChange(CefRefPtr<CefBrowser> Browser, const CefString& Title) override;
	virtual void OnAddressChange(CefRefPtr<CefBrowser> Browser, CefRefPtr<CefFrame> Frame, const CefString& Url) override;
	virtual bool OnTooltip(CefRefPtr<CefBrowser> Browser, CefString& Text) override;

public:

	// CefLifeSpanHandler Interface

	virtual void OnAfterCreated(CefRefPtr<CefBrowser> Browser) override;
	virtual void OnBeforeClose(CefRefPtr<CefBrowser> Browser) override;
	virtual bool OnBeforePopup(CefRefPtr<CefBrowser> Browser, 
		CefRefPtr<CefFrame> Frame, 
		const CefString& Target_Url, 
		const CefString& Target_Frame_Name,
		const CefPopupFeatures& PopupFeatures, 
		CefWindowInfo& WindowInfo,
		CefRefPtr<CefClient>& Client, 
		CefBrowserSettings& Settings,
		bool* no_javascript_access)  override;

public:

	// CefLoadHandler Interface

	virtual void OnLoadError(CefRefPtr<CefBrowser> Browser,
		CefRefPtr<CefFrame> Frame,
		CefLoadHandler::ErrorCode InErrorCode,
		const CefString& ErrorText,
		const CefString& FailedUrl) override;

	virtual void OnLoadingStateChange(
		CefRefPtr<CefBrowser> browser,
		bool isLoading,
		bool canGoBack,
		bool canGoForward) override;

	virtual void OnLoadStart(CefRefPtr<CefBrowser> Browser, CefRefPtr<CefFrame> Frame) override;


public:

	// CefRenderHandler Interface

	virtual bool GetViewRect(CefRefPtr<CefBrowser> Browser, CefRect& Rect) override;
	virtual void OnPaint(CefRefPtr<CefBrowser> Browser,
		PaintElementType Type,
		const RectList& DirtyRects,
		const void* Buffer,
		int Width, int Height) override;
	virtual void OnCursorChange(CefRefPtr<CefBrowser> Browser,
		CefCursorHandle Cursor,
		CefRenderHandler::CursorType Type,
		const CefCursorInfo& CustomCursorInfo) override;

public:

	// CefRequestHandler Interface

	virtual bool OnBeforeResourceLoad(CefRefPtr<CefBrowser> Browser,
		CefRefPtr<CefFrame> Frame,
		CefRefPtr<CefRequest> Request) override;
	virtual void OnRenderProcessTerminated(CefRefPtr<CefBrowser> Browser, TerminationStatus Status) override;
	virtual bool OnBeforeBrowse(CefRefPtr<CefBrowser> Browser,
		CefRefPtr<CefFrame> Frame,
		CefRefPtr<CefRequest> Request,
		bool IsRedirect) override;
	virtual CefRefPtr<CefResourceHandler> GetResourceHandler( CefRefPtr<CefBrowser> Browser, CefRefPtr< CefFrame > Frame, CefRefPtr<CefRequest> Request ) override;
public:
	// CefMessageRouterBrowserSide::Handler Interface
	
	virtual bool OnQuery(CefRefPtr<CefBrowser> Browser,
		CefRefPtr<CefFrame> Frame,
		int64 QueryId,
		const CefString& Request,
		bool Persistent,
		CefRefPtr<CefMessageRouterBrowserSide::Callback> Callback) override;

	virtual void OnQueryCanceled(CefRefPtr<CefBrowser> Browser,
		CefRefPtr<CefFrame> Frame,
		int64 QueryId) override;

public:
	// CefKeyboardHandler interface
	virtual bool OnKeyEvent(CefRefPtr<CefBrowser> Browser,
		const CefKeyEvent& Event,
		CefEventHandle OsEvent) override;

private:

	/** Weak Pointer to our Web Browser window so that events can be passed on while it's valid*/
	TWeakPtr<FWebBrowserWindow> BrowserWindowPtr;
	
	/** Weak Pointer to the parent web browser window */
	TWeakPtr<FWebBrowserWindow> BrowserWindowParentPtr;

	/** Whether to show an error message in case of loading errors. */
	bool ShowErrorMessage;

	/** The message router is used as a part of a generic message api between Javascript in the render process and the application process */
	CefRefPtr<CefMessageRouterBrowserSide> MessageRouter;

	// Include the default reference counting implementation.
	IMPLEMENT_REFCOUNTING(FWebBrowserHandler);
};

#endif
