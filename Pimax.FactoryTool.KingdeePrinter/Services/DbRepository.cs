using Pimax.FactoryTool.KingdeePrinter.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Pimax.FactoryTool.KingdeePrinter.Services
{
    public class DbRepository : DbContext
    {
        public DbSet<MaterialBinding> MaterialBindings { get; set; }
        public DbSet<ProductInfo> ProductInfos { get; set; }

        public async Task<List<MaterialBinding>> GetAllBindingsAsync(string serialNumber)
        {
            return await MaterialBindings.Where(m => m.SerialNumber == serialNumber).ToListAsync();
        }

        public async Task<MaterialBinding> GetMaterialBindingBySNAsync(string serialNumber)
        {
            return await MaterialBindings.FirstOrDefaultAsync(m => m.SerialNumber == serialNumber && m.BindingType == AppConfig.BindingType);
        }

        public async Task<MaterialBinding> GetMaterialBindingByKNAsync(string kn)
        {
            return await MaterialBindings.FirstOrDefaultAsync(m => m.BindingNumber == kn && m.BindingType == AppConfig.BindingType);
        }

        public async Task InsertBindingAsync(MaterialBinding binding)
        {
            var id = await MaterialBindings.MaxAsync(m => m.Id);
            binding.Id = id + 1;    //参照老版本绑定工具写法
            MaterialBindings.Add(binding);
        }

        public Task DeleteBindingsAsync(IEnumerable<MaterialBinding> bindings)
        {
            foreach (var m in bindings)
            {
                MaterialBindings.Attach(m);
                MaterialBindings.Remove(m);
            }

            return Task.CompletedTask;
        }

        public Task DeleteProductInfoAsync(IEnumerable<ProductInfo> productInfos)
        {
            foreach (var p in productInfos)
            {
                ProductInfos.Attach(p);
                ProductInfos.Remove(p);
            }
            
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<string>> GetAllProductCodeAsync()
        {
            return await ProductInfos.Select(m => m.ProductCode).Distinct().ToListAsync();
        }

        public void InsertProductInfo(ProductInfo info) => ProductInfos.Add(info);
    }
}
