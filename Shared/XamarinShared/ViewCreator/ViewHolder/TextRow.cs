namespace XamarinShared.ViewCreator.ViewHolder
{
    public class TextRow:BaseRowViewholder
    {
        public string OriginalMessage { get; set; }
        public string TranslatedMessage { get; set; }

        public TextRow() : base(RowType.INCOMING_TEXT)
        {
            
        }
    }
}