using System.Collections.Generic;
using System.Linq;

namespace Paycor.Import.MapFileImport.Implementation.Preparer
{
    public abstract class BasePreparePayloadFactory : IPreparePayloadFactory
    {
        private readonly List<IPayloadExtracter> _preparePayloadExtracters = new List<IPayloadExtracter>();

        public abstract void LoadHandlers();

        public IPayloadExtracter GetPayloadExtracter(PreparePayloadTypeEnum preparePayloadTypeEnum)
        {
            return _preparePayloadExtracters.Single(t => t.GetPreparePayloadType() == preparePayloadTypeEnum);
        }

        protected void AddPreparePayloadExtracters(IEnumerable<IPayloadExtracter> preparePayloadExtracters)
        {
            _preparePayloadExtracters.AddRange(preparePayloadExtracters);
        }
    }
}