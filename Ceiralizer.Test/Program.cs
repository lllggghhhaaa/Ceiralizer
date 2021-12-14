// Copyright 2022 lllggghhhaaa
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

using System.Text;
using Ceiralizer;
using Ceiralizer.Test;

// Default is UNICODE, change to UTF8 to reduce the size.
PacketSerializer.StringEncoder = Encoding.UTF8;

IEnumerable<byte> data = PacketSerializer.Serialize(new CeiraPacket
{
    Op = 2,
    Value = 255,
    Garbage = "Lixo",
    Prefix = 'c',
    Ceirinha = new CeirinhaPacket
    {
        Ceirinha = "Ceira purinha"
    },
    Name = "Ceira",
    IsWorking = true
});

CeiraPacket packet = PacketSerializer.Deserialize<CeiraPacket>(data);

Console.WriteLine(packet.Op);
Console.WriteLine(packet.Value);
Console.WriteLine(packet.Prefix);
Console.WriteLine(packet.Ceirinha.Ceirinha);
Console.WriteLine(packet.Name);
Console.WriteLine(packet.IsWorking);

Console.WriteLine();

foreach (byte b in data)
    Console.Write(Convert.ToString(b, 2).PadLeft(8, '0') + ' ');