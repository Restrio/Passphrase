using System.Drawing;

namespace Passphrase;

[Serializable]
public record struct Config(string Passphrase, double Opacity, Location Location, Size Size);