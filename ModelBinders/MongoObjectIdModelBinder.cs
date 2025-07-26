using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;

namespace BankingApplication.ModelBinders
{
    public class MongoObjectIdModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
                return Task.CompletedTask;

            if (!ObjectId.TryParse(value, out var objectId))
            {
                bindingContext.ModelState.AddModelError(modelName, "Invalid id format");
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(objectId);

            return Task.CompletedTask;
        }
    }
}