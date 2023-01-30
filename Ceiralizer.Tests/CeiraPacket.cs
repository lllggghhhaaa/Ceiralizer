// Copyright 2023 lllggghhhaaa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
// limitations under the License.

namespace Ceiralizer.Tests;

public struct CeiraPacket : IPacket
{
    [PacketField] public short Op;

    public string Garbage;

    [PacketField] public int Value;

    [PacketField] public int[] Ceiras;

    [PacketField] public char Prefix;

    [PacketField] public CeirinhaPacket Ceirinha;

    [PacketField] public string Name;

    [PacketField] public bool IsWorking;

    [PacketField] public Vector2 Position;

    [PacketField] public Ceirax Ceirax;
}

public struct CeirinhaPacket : IPacket
{
    [PacketField] public string Ceirinha;
}

public struct Vector2
{
    public int X;
    public int Y;

    public Vector2(int x = 0, int y = 0)
    {
        X = x;
        Y = y;
    }
}