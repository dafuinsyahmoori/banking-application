using BankingApplication.ModelBinders;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using MongoDB.Bson;

namespace BankingApplication.ModelBinderProviders
{
    public class MongoObjectIdModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.Metadata.ModelType == typeof(ObjectId))
                return new BinderTypeModelBinder(typeof(MongoObjectIdModelBinder));

            return null;
        }
    }
}