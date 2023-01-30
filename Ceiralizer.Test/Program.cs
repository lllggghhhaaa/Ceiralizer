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

// Add custom class to serialize
TypeSerializer.Serializers.Add(typeof(Vector2), value =>
{
    List<byte> bytes = new List<byte>();

    Vector2 pos = (Vector2) value;

    byte[] x = TypeSerializer.Serializers[typeof(int)].Invoke(pos.X);
    byte[] y = TypeSerializer.Serializers[typeof(int)].Invoke(pos.Y);

    bytes.AddRange(x);
    bytes.AddRange(y);

    return bytes.ToArray();
});

// And also adding deserializer
TypeSerializer.Deserializers.Add(typeof(Vector2), data =>
{
    int x = data.ReadInt();
    int y = data.ReadInt();

    return new Vector2(x, y);
});

List<byte> data = PacketSerializer.Serialize(new CeiraPacket
{
    Op = 2,
    Value = 255,
    Ceiras = new[] { 4, 66, 95 },
    Garbage = "Lixo",
    Prefix = 'c',
    Ceirinha = new CeirinhaPacket
    {
        Ceirinha = "Ceira purinha"
    },
    Name = "Ceira",
    IsWorking = true,
    Position = new Vector2(5, 25),
    Ceirax = new Ceirax
    {
        Title = "In The End, We All Felt Like We Ate Too Much",
        Description = "North America should be called Russia since people are always moving so fast. Gralitica Cemeteries are just garbage dumps filled with humans I'm still upset that Tie Domi didn't name his child Tyson ",
        Price = 58329.32f
    }
}).ToList();

CeiraPacket packet = PacketSerializer.Deserialize<CeiraPacket>(data);

Console.WriteLine(packet.Op);
Console.WriteLine(packet.Value);

foreach (int ceira in packet.Ceiras) Console.Write($" {ceira} ");
Console.WriteLine();

Console.WriteLine(packet.Prefix);
Console.WriteLine(packet.Ceirinha.Ceirinha);
Console.WriteLine(packet.Name);
Console.WriteLine(packet.IsWorking);
Console.WriteLine($"X: {packet.Position.X}, Y: {packet.Position.Y}");
Console.WriteLine();
Console.WriteLine(packet.Ceirax.Title);
Console.WriteLine(packet.Ceirax.Description);
Console.WriteLine(packet.Ceirax.Price);

Console.WriteLine();

foreach (byte b in data)
    Console.Write(Convert.ToString(b, 2).PadLeft(8, '0') + ' ');

Console.WriteLine();