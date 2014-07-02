// Copyright 1998-2014 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "Slate.h"
#include "SScrubWidget.h"
#include "Editor/EditorWidgets/Public/ITransportControl.h"

class KISMETWIDGETS_API SScrubControlPanel : public SCompoundWidget
{
public:

	SLATE_BEGIN_ARGS(SScrubControlPanel)
		: _Value(0)
		, _NumOfKeys()
		, _SequenceLength()
		, _OnValueChanged()
		, _OnBeginSliderMovement()
		, _OnEndSliderMovement()
		, _OnClickedForwardPlay()
		, _OnClickedRecord()
		, _OnClickedBackwardPlay()
		, _OnClickedForwardStep()
		, _OnClickedBackwardStep()
		, _OnClickedForwardEnd()
		, _OnClickedBackwardEnd()
		, _OnClickedToggleLoop()
		, _OnGetLooping()
		, _OnGetPlaybackMode()
		, _ViewInputMin()
		, _ViewInputMax()
		, _OnSetInputViewRange()
		, _OnCropAnimSequence()
	{}
		SLATE_ATTRIBUTE( float, Value )
		SLATE_ATTRIBUTE( uint32, NumOfKeys )
		SLATE_ATTRIBUTE( float, SequenceLength )
		SLATE_ARGUMENT( bool, bAllowZoom )
		/** Called when the value is changed by slider or typing */
		SLATE_EVENT( FOnFloatValueChanged, OnValueChanged )
		/** Called right before the slider begins to move */
		SLATE_EVENT( FSimpleDelegate, OnBeginSliderMovement )
		/** Called right after the slider handle is released by the user */
		SLATE_EVENT( FOnFloatValueChanged, OnEndSliderMovement )
		/** Called for slider **/
		//SLATE_EVENT(TAttribute<float>::FGetter, OnValueRead)		
		/** Called when the button is clicked */
		SLATE_EVENT( FOnClicked, OnClickedForwardPlay )
		SLATE_EVENT( FOnClicked, OnClickedRecord )
		SLATE_EVENT( FOnClicked, OnClickedBackwardPlay )
		SLATE_EVENT( FOnClicked, OnClickedForwardStep )
		SLATE_EVENT( FOnClicked, OnClickedBackwardStep )
		SLATE_EVENT( FOnClicked, OnClickedForwardEnd )
		SLATE_EVENT( FOnClicked, OnClickedBackwardEnd )
		SLATE_EVENT( FOnClicked, OnClickedToggleLoop )
		SLATE_EVENT( FOnGetLooping, OnGetLooping )
		SLATE_EVENT( FOnGetPlaybackMode, OnGetPlaybackMode )
		/** View Input range **/
		SLATE_ATTRIBUTE( float, ViewInputMin )
		SLATE_ATTRIBUTE( float, ViewInputMax )
		SLATE_EVENT( FOnSetInputViewRange, OnSetInputViewRange )
		/** Called when an anim sequence is cropped before/after a selected frame */
		SLATE_EVENT( FOnCropAnimSequence, OnCropAnimSequence )
		/** Called to zero out selected frame's translation from origin */
		SLATE_EVENT( FSimpleDelegate, OnReZeroAnimSequence )
		SLATE_ATTRIBUTE( bool, IsRealtimeStreamingMode )
		/** Optional, additional values to draw on the timeline **/
		SLATE_ATTRIBUTE( TArray<float>, DraggableBars )
		SLATE_EVENT( FOnScrubBarDrag, OnBarDrag)
	SLATE_END_ARGS()

	/**
	 * Construct the widget
	 * 
	 * @param InArgs   A declaration from which to construct the widget
	 */
	void Construct( const FArguments& InArgs );
protected:
	EVisibility GetRealtimeControlVisibility(bool bIsControlForRealtimeStreamingMode) const;

protected:
	TSharedPtr<class SScrubWidget>	ScrubWidget;

	TAttribute<bool> IsRealtimeStreamingMode;
};

