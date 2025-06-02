
using Microsoft.ML;
using Microsoft.ML.Data;

namespace FastBite.Implementation.Classes;
public class TagTransformer : IEstimator<ITransformer>
{
    public ITransformer Fit(IDataView input) => new TagTransformerImpl();

    public SchemaShape GetOutputSchema(SchemaShape inputSchema) => inputSchema;

    private class TagTransformerImpl : ITransformer
    {
        public bool IsRowToRowMapper => false;

        public DataViewSchema GetOutputSchema(DataViewSchema inputSchema) => inputSchema;
        
        public IDataView Transform(IDataView input) => input;
                
        public void Save(ModelSaveContext ctx) { }

        public IRowToRowMapper GetRowToRowMapper(DataViewSchema inputSchema)
        {
            throw new NotImplementedException();
        }
    }
}