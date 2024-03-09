using EncryptedMessaging;

namespace XamarinShared.ViewCreator.ViewHolder
{
    public abstract class BaseRowViewholder
    {
        public RowType RowType;
        public Message message;
        
        protected BaseRowViewholder(RowType rowType)
        {
            RowType = rowType;
        }
    }

    public class DateRow : BaseRowViewholder
    {
        public DateRow() : base(RowType.DATE_HEADER)
        {   
            
        }
    }

    public enum RowType
    {
        DATE_HEADER,
        INCOMING_TEXT,
        OUTGOING_TEXT
        
    }
}