namespace Shared.Interfaces;
public interface IWriteable {
    public void Write(Writer w, Span<byte> b);
}
