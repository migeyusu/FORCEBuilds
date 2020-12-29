using System.Collections.Generic;
using Castle.Components.DictionaryAdapter;

namespace FORCEBuild.Persistence
{
    public abstract class DataBaseCommand
    {
        public string TableName { get; set; }
    }

    public class InsertCommand : DataBaseCommand
    {
        public string IdColumn => "guid";
        public List<ColumnValuePair> InsertPairs { get; set; }

        public InsertCommand()
        {
            InsertPairs=new EditableList<ColumnValuePair>();
        }
    }

    public class UpdateCommand : DataBaseCommand
    {
        public List<ColumnValuePair> UpdatePairs { get; set; }
        public List<ColumnValuePair> ConditionPairs { get; set; }

        public UpdateCommand()
        {
            UpdatePairs = new EditableList<ColumnValuePair>();
            ConditionPairs = new List<ColumnValuePair>();
        }
    }

    public class DeleteCommand : DataBaseCommand
    {
        public List<ColumnValuePair> ConditionPairs { get; set; }

        public DeleteCommand()
        {
            ConditionPairs=new List<ColumnValuePair>();
        }
    }

    public class SelectCommand : DataBaseCommand
    {
        public List<ColumnValuePair> ConditionPairs { get; set; }

        public SelectCommand()
        {
            ConditionPairs=new List<ColumnValuePair>();
        }
    }
}
