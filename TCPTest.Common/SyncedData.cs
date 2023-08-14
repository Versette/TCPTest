using System.Collections.ObjectModel;
using System.ComponentModel;
using MessagePack;

namespace TCPTest.Common;

[MessagePackObject]
public class SyncedData : INotifyPropertyChanged
{
    [Key(0)] public int Id { get; set; }

    [Key(1)] public ObservableCollection<string> Data { get; set; } = new();

    [Key(2)] public byte[] RandomBinaryData { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}