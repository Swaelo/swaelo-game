﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClientPacketType
{
    ConsoleMessage = 1,    //send a message to be displayed in servers console window
    PlayerMessage = 2,  //spread a player chat message to other clients chat windows

    RegisterRequest = 3,   //request to register a new account
    LoginRequest = 4,      //request to log into an account
    CreateCharacterRequest = 5, //request to create a new character and save it under our account
    EnterWorldRequest = 6, //tell client to enter the game world with their selected character
    GetCharacterDataRequest = 7,   //get information regarding all characters created under our account

    PlayerUpdatePosition = 8,   //spread a players position update info to other clients
    PlayerDisconnectNotice = 9,  //let the server know when we are leaving the game
    AccountLogoutNotice = 10    //let the server know we have logged out of this user account
}

//sends packets to the game server
public class PacketSender : MonoBehaviour
{
    public static PacketSender Instance;
    private ServerConnection connection;
    private void Awake() { Instance = this; connection = GetComponent<ServerConnection>(); }

    //Sends a packet to the server after its been filled with data from one of the other functions in this class
    private void SendPacket(byte[] PacketData)
    {
        ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();   //start the packet writer
        PacketWriter.WriteBytes(PacketData);    //fill it with data
        //Make sure the connection to the server is still open
        if(!connection.IsServerConnected() || !connection.ClientStream.CanRead)
        {
            ChatWindow.Instance.DisplayErrorMessage("couldnt send packet, connection to the server is no longer open");
            return;
        }
        //send the packet to the server
        connection.ClientStream.Write(PacketWriter.ToArray(), 0, PacketWriter.ToArray().Length);
        PacketWriter.Dispose(); //close the packet writer
    }

    //Sends a message to the server <int:PacketType, string:Message>
    public void SendConsoleMessage(string Message)
    {
        //Make the packet that will be sent
        ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
        PacketWriter.WriteInteger((int)ClientPacketType.ConsoleMessage);
        PacketWriter.WriteString(Message);
        //Send the packet to the server
        SendPacket(PacketWriter.ToArray());
        //Close the packet writer
        PacketWriter.Dispose(); //close the packet writer
    }

    //Sends our chat message to the server to be delivered to all the other clients
    public void SendPlayerMessage(string Message)
    {
        ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();
        PacketWriter.WriteInteger((int)ClientPacketType.PlayerMessage);
        PacketWriter.WriteString(PlayerInfo.CharacterName);
        PacketWriter.WriteString(Message);
        SendPacket(PacketWriter.ToArray());
        PacketWriter.Dispose();
    }

    //Sends a request to the server to register a new account <int:PacketType, string:Username, string:Password>
    public void SendRegisterRequest(string username, string password)
    {
        ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();   //start the packet writer
        PacketWriter.WriteInteger((int)ClientPacketType.RegisterRequest);  //write the packet type
        //write the account credentials
        PacketWriter.WriteString(username);
        PacketWriter.WriteString(password);
        SendPacket(PacketWriter.ToArray()); //send the packet to the server
        PacketWriter.Dispose(); //close the packet writer
    }

    //Sends a request to the server to log into an account <int:PacketType, string:Username, string:Password>
    public void SendLoginRequest(string username, string password)
    {
        ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();   //start the packet writer
        PacketWriter.WriteInteger((int)ClientPacketType.LoginRequest); //write the packet type
        //write the account credentials
        PacketWriter.WriteString(username);
        PacketWriter.WriteString(password);
        SendPacket(PacketWriter.ToArray()); //send the packet to the server
        PacketWriter.Dispose(); //close the packet writer
    }

    //Sends a request to the server to create a new character registered to our account
    public void SendCreateCharacterRequest(string AccountName, string CharacterName, bool IsMale)
    {
        ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();   //start the packet writer
        PacketWriter.WriteInteger((int)ClientPacketType.CreateCharacterRequest); //write the packet type
        PacketWriter.WriteString(AccountName);
        PacketWriter.WriteString(CharacterName);
        PacketWriter.WriteInteger(IsMale ? 1 : 0);
        SendPacket(PacketWriter.ToArray());
        PacketWriter.Dispose();
    }

    //Sends a request to the server for us to be sent into the game world with the selected character
    public void SendEnterWorldRequest(CharacterData Data)
    {
        ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();   //start the packet writer
        PacketWriter.WriteInteger((int)ClientPacketType.EnterWorldRequest); //write the packet type
        PacketWriter.WriteString(Data.Account);
        PacketWriter.WriteString(Data.Name);
        PacketWriter.WriteFloat(Data.Position.x);
        PacketWriter.WriteFloat(Data.Position.y);
        PacketWriter.WriteFloat(Data.Position.z);
        SendPacket(PacketWriter.ToArray());
        PacketWriter.Dispose();
    }

    //Sends a request to the server to return all the data about any characters we have created so far
    public void SendGetCharacterDataRequest(string username)
    {
        ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();   //start the packet writer
        PacketWriter.WriteInteger((int)ClientPacketType.GetCharacterDataRequest); //write the packet type
        PacketWriter.WriteString(username);
        SendPacket(PacketWriter.ToArray());
        PacketWriter.Dispose();
    }
    
    //Sends information to the server updating them on our players current location <int:PacketType, string:AccountName, vector3:position, vector4:rotation>
    public void SendPlayerUpdate(Vector3 Position, Quaternion Rotation)
    {
        //Console.Instance.Print("telling the server our players updated position information");
        ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();   //start the packet writer
        PacketWriter.WriteInteger((int)ClientPacketType.PlayerUpdatePosition);  //write the packet type
        PacketWriter.WriteString(PlayerInfo.CharacterName);
        //write the position data
        PacketWriter.WriteFloat(Position.x);
        PacketWriter.WriteFloat(Position.y);
        PacketWriter.WriteFloat(Position.z);
        //write rotation data
        PacketWriter.WriteFloat(Rotation.x);
        PacketWriter.WriteFloat(Rotation.y);
        PacketWriter.WriteFloat(Rotation.z);
        PacketWriter.WriteFloat(Rotation.w);
        //send the packet and close the writer
        SendPacket(PacketWriter.ToArray());
        PacketWriter.Dispose();
    }

    //Tells the server we are leaving the game now
    public void SendDisconnectNotice()
    {
        //Console.Instance.Print("telling the server our players updated position information");
        ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();   //start the packet writer
        PacketWriter.WriteInteger((int)ClientPacketType.PlayerDisconnectNotice);  //write the packet type
        //send and close the packet
        SendPacket(PacketWriter.ToArray());
        PacketWriter.Dispose();
    }

    //Tells the server we are logging out of this user account
    public void SendAccountLogoutNotice()
    {
        //Console.Instance.Print("telling the server our players updated position information");
        ByteBuffer.ByteBuffer PacketWriter = new ByteBuffer.ByteBuffer();   //start the packet writer
        PacketWriter.WriteInteger((int)ClientPacketType.AccountLogoutNotice);  //write the packet type
        //send and close the packet
        SendPacket(PacketWriter.ToArray());
        PacketWriter.Dispose();
    }
}