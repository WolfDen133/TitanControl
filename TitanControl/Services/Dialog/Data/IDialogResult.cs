namespace TitanControl.Services.Dialog.Data
{
    public interface IDialogResult<T>
    {
        public T Value { get; protected set; }
    }
}
