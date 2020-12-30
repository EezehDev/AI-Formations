// Copyright Epic Games, Inc. All Rights Reserved.

#include "FormationsGameMode.h"
#include "FormationsPlayerController.h"
#include "FormationsCharacter.h"
#include "UObject/ConstructorHelpers.h"

AFormationsGameMode::AFormationsGameMode()
{
	// use our custom PlayerController class
	PlayerControllerClass = AFormationsPlayerController::StaticClass();

	// set default pawn class to our Blueprinted character
	static ConstructorHelpers::FClassFinder<APawn> PlayerPawnBPClass(TEXT("/Game/TopDownCPP/Blueprints/TopDownCharacter"));
	if (PlayerPawnBPClass.Class != NULL)
	{
		DefaultPawnClass = PlayerPawnBPClass.Class;
	}
}