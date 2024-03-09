namespace XamarinShared.ViewCreator.ViewHolder
{
    public class OutgoingTextRow:BaseRowViewholder
    {
        public string OriginalMessage { get; set; }
        public string TranslatedMessage { get; set; }

        public OutgoingTextRow() : base(RowType.INCOMING_TEXT)
        {
        }
    }
}