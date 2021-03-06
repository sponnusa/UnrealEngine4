// Copyright 1998-2015 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "PaperSprite.h"
#include "SpriteDrawCall.generated.h"

//
USTRUCT()
struct FSpriteDrawCallRecord
{
public:
	GENERATED_USTRUCT_BODY()

	UPROPERTY()
	FVector Destination;

	UPROPERTY()
	UTexture* BaseTexture;

	FAdditionalSpriteTextureArray AdditionalTextures;

	UPROPERTY()
	FColor Color;

	//@TODO: The rest of this struct doesn't need to be properties either, but has to be due to serialization for now
	// Render triangle list (stored as loose vertices)
	TArray< FVector4, TInlineAllocator<6> > RenderVerts;

	void BuildFromSprite(const class UPaperSprite* Sprite);

	FSpriteDrawCallRecord()
		: BaseTexture(nullptr)
		, Color(FColor::White)
	{
	}

	bool IsValid() const
	{
		return (RenderVerts.Num() > 0) && (BaseTexture != nullptr) && (BaseTexture->Resource != nullptr);
	}
};
