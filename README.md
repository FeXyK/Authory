# Authory
 This project was made at Budapest University of Technology and Eonomics for my BSc degree as an IT Engineer

# README
A szerver futtatásához szükség van egy MySql (verzió 8.0.22) szerverre és egy hozzá
tartozó felhasználóra, aminek az autentikációs típusa „Standard”. A gyökér könyvtárban
található „CreateAuthorySchema.sql” SQL query futtatásával létrehozhatod az adatbázis
sémát
Mindkét szervernek van külön konfigurációs fájlja. Amennyiben ezt nem találja, az első
indításnál be fogja kérni a szoftver az adatokat, majd kimenti azt a futtatott .exe fájl mellé,
ha Windows-on futtatod.
A kliensben (bal alsó sarok) lehetőséged van a bejelentkező képernyőn beállítani a
MasterServer IP címét, és a portját.
AuthoryServer konfigurációs fájl:

# AuthoryServer config file
#### #Server details:

- MapsFolderPath: Azt határozza meg, hogy a szerver hol keresse a szerver oldali
entitások betöltési fájlját, ez egy .spawner kiterjesztésű fájl.
- ServerAuthString: Ennek a string-nek egyeznie kell a kliensen található
AuthoryClient-ben található AuthString-gel, egyébként a Lidgren nem ismeri fel a
kapcsolatot.
- IsServerRunningLocalHost: Értéke True/False, azt határozza meg, hogy a szerver a
hálózat külső IP címét adja-e meg a MesterServer számára, vagy a belső IP címét.
Amennyiben az értéke hamis egy külső weboldalról (https://api.ipify.org) fogja letölteni a külső IP címet
- ServerPort: A szerver csomópont kezdő portját határozza meg. Mindenképpen más legyen mint az AuthoryMasterServer-ben beállított.

#### #MasterServer details:
- MasterServerAuthString: A MasterServer-en belül az AuthString-nél ugyanennek a stringnek kell
szerepelnie.
- MasterServerHost: Megadja, hogy a csomópont milyen IP címen keresse az
AuthoryMasterServer-t
- MasterServerPort: Az a port amin az AuthoryMasterServer fut.


# AuthoryMasterServer config file
#### #Server details:
- ServerAuthString: Egyezni kell az AuthoryServer konfigurációs fájlban beállított
MasterServerAuthString-gel
- ServerPort: A szerver ezen porton fog elindulni
#### #DatabaseDetails:
- DBServer: Az adatbázis (MySql) szerver IP címe
- DBName: Az adatbázis séma  ("authory", ha a mellékelt sql query-t használod)
- DBUser: Az adatbázis sémában található táblákhoz hozzáféréssel rendelkező felhasználó
- DBPassword: A megadott felhasználóhoz tartozó jelszó

# Pictures from the game

![](Images/LoginScreen "LoginScreen")
![](Images/Ingame "Ingame")
