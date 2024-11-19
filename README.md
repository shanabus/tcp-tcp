# tcp-tcp
TCP Table Coordination Program

Used to coordinate humans in their semi-prone positions

## Publishing (with current provider)
```
dotnet publish -c Release -p:PublishProfile="Profile-Name-FTP" 
```
That gives an error so probably missing the Profile object in the SLN or folder so then just havae to navigate to the Server bin and find the publish.zip

`..\Server\bin\Release\net7.0\publish`

1. Upload it through File Manager
2. Delete all the other files
3. Extract new publish.zip
4. Move all files up a level

Test!