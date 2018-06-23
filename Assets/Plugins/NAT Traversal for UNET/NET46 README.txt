In order to enable .NET 4.6 support you'll need to change the import settings on NATTraversalForUNet46.dll and Open.Nat45.dll so that they are included.
You'll also need to change the import settings on NATTraversalForUNet.dll, Open.Nat.dll, and System.Threading.dll so that they are excluded.


You will then unfortunately have to re-assign any NATHelper, NATTraversal.MigrationManager, or NATTraversal.NetworkManager scripts on any game objects.
Usually this just means re-assigning the NATHelper script since the others are generally extended from and not assigned directly.