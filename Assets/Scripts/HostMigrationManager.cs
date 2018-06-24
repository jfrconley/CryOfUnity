#if !DISABLE_NAT_TRAVERSAL
#if !UNITY_5_2

using UnityEngine;
using System.Collections;
using UnityEngine.Networking.Types;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using NATTraversal;
using System;
using System.IO;

public class ExampleMigrationManager : NATTraversal.MigrationManager
{

    void OnGUI()
    {
        if (hostWasShutdown)
        {
            OnGUIHost();
            return;
        }

        if (disconnectedFromHost && oldServerConnectionId != -1)
        {
            OnGUIClient();
        }
    }

    void OnGUIHost()
    {
        int ypos = 310;
        const int spacing = 25;

        GUI.Label(new Rect(10, ypos, 200, 40), "Host Was Shutdown ID(" + oldServerConnectionId + ")");
        ypos += spacing;

        if (waitingReconnectToNewHost)
        {
            if (GUI.Button(new Rect(10, ypos, 200, 20), "Reconnect as Client"))
            {
                Reset(ClientScene.ReconnectIdHost);
                networkManager.networkAddress = newHostAddress;
                networkManager.StartClientAll(newHost.address, newHost.internalIP, newHost.port, newHost.guid, NetworkID.Invalid, newHost.externalIPv6, newHost.internalIPv6);
            }
            ypos += spacing;
        }
        else
        {
            if (GUI.Button(new Rect(10, ypos, 200, 20), "Pick New Host"))
            {
                bool youAreNewHost;
                if (FindNewHost(out newHost, out youAreNewHost))
                {
                    newHostAddress = newHost.address;
                    if (youAreNewHost)
                    {
                        // you cannot be the new host.. you were the old host..?
                        Debug.LogWarning("MigrationManager FindNewHost - new host is self?");
                    }
                    else
                    {
                        waitingReconnectToNewHost = true;
                    }
                }
            }
            ypos += spacing;
        }

        if (GUI.Button(new Rect(10, ypos, 200, 20), "Leave Game"))
        {
            networkManager.SetupMigrationManager(null);
            networkManager.StopHost();

            Reset(ClientScene.ReconnectIdInvalid);
        }
    }

    void OnGUIClient()
    {
        int ypos = 300;
        const int spacing = 25;

        GUI.Label(new Rect(10, ypos, 200, 40), "Lost Connection To Host ID(" + oldServerConnectionId + ")");
        ypos += spacing;

        if (waitingToBecomeNewHost)
        {
            GUI.Label(new Rect(10, ypos, 200, 40), "You are the new host");
            ypos += spacing;

            if (GUI.Button(new Rect(10, ypos, 200, 20), "Start As Host"))
            {
                NetworkServer.Configure(networkManager.topo);

                string connectionInfo = networkManager.GetMatchNameFromConnectionInfo();
                networkManager.matchMaker.CreateMatch(connectionInfo, networkManager.matchSize, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
                BecomeNewHost(networkManager.networkPort);
            }
            ypos += spacing;
        }
        else if (waitingReconnectToNewHost)
        {
            GUI.Label(new Rect(10, ypos, 200, 40), "New host is " + newHostAddress);
            ypos += spacing;

            if (GUI.Button(new Rect(10, ypos, 200, 20), "Reconnect To New Host"))
            {
                ReconnectToNewHost();
            }
            ypos += spacing;
        }
        else
        {
            if (GUI.Button(new Rect(10, ypos, 200, 20), "Pick New Host"))
            {
                bool youAreNewHost;
                if (FindNewHost(out newHost, out youAreNewHost))
                {
                    newHostAddress = newHost.address;
                    if (youAreNewHost)
                    {
                        waitingToBecomeNewHost = true;
                    }
                    else
                    {
                        waitingReconnectToNewHost = true;
                    }
                }
            }
            ypos += spacing;
        }

        if (GUI.Button(new Rect(10, ypos, 200, 20), "Leave Game"))
        {
            networkManager.SetupMigrationManager(null);
            if (NetworkServer.active)
            {
                networkManager.StopHost();
            }
            else if (NetworkClient.active)
            {
                networkManager.StopClient();
            }

            Reset(ClientScene.ReconnectIdInvalid);
        }
    }
}
#endif
#endif