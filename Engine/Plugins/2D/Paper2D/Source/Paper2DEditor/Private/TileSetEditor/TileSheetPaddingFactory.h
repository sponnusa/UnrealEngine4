// Copyright 1998-2015 Epic Games, Inc. All Rights Reserved.

#pragma once

class UPaperTileSet;

#include "TileSheetPaddingFactory.generated.h"

/**
 * Factory used to pad out each individual tile in a tile sheet texture
 */

UCLASS()
class UTileSheetPaddingFactory : public UFactory
{
	GENERATED_UCLASS_BODY()

	// Source tile sheet texture
	UPROPERTY()
	UPaperTileSet* SourceTileSet;

	// The amount to extrude out from each tile (in pixels)
	UPROPERTY(Category = Settings, EditAnywhere, meta=(UIMin=1, ClampMin=1))
	int32 ExtrusionAmount;

	// Should we pad the texture to the next power of 2?
	UPROPERTY(Category=Settings, EditAnywhere)
	bool bPadToPowerOf2;


	// UFactory interface
	virtual UObject* FactoryCreateNew(UClass* Class, UObject* InParent, FName Name, EObjectFlags Flags, UObject* Context, FFeedbackContext* Warn) override;
	// End of UFactory interface
};