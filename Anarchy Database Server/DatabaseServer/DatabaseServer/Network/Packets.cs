public enum ServerPackets {
    GS_WelcomeMessage = 1,
    GS_InGame,
    GS_PlayerData,
    GS_PlayerPosition,
    GS_PlayerRotation,
    GS_WorldWeapons,
    GS_WorldAmmo,
    GS_PlayerPickedUpItem,
    GS_UpdateAmmo,
    GS_PlayerShot,
    GS_PlayerSwitchedWeapon,
    GS_PlayerDied,
    GS_PlayerDisconnected,
    GS_Respawn,

    DB_UsernameExistsError,
    DB_EmailExistsError,
    DB_IncorrectLoginDetails,
    DB_ConfirmLoginDetails,
}

public enum ClientPackets {
    CPlayerPosition = 1,
    CPlayerRotation,
    CPlayerLoaded,
    CRegisterPlayer,
    CMatchmakeRequest,
    CRequestWorldObjects,
    CPickedUpItem,
    CPlayerReload,
    CPlayerShoot,
    CDamagedEnemy,
    CSwitchWep,
    CPlayerRespawn,

    C_LoginCredentials,
    C_CreateAccountDetails,
}