using Dapper;
using order.Context;
using order.DTOModel;
using order.IRepository.IAdminRepositorys;
using order.Models;
using order.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace order.Repository.AdminRepository
{
    public class ProductRepo : IProductRepo
    {
        private readonly DapperContext _dapperContext;
        private readonly IBrandRepo _brandRepo;

        public ProductRepo(DapperContext dapperContext, IBrandRepo brandRepo)
        {
            _dapperContext = dapperContext;
            _brandRepo = brandRepo;
        }
        public static string IncrementNumber(string input)
        {
            // Find the position where the number starts
            int numberStartIndex = input.IndexOfAny("0123456789".ToCharArray());
            if (numberStartIndex == -1)
            {
                throw new ArgumentException("Input string does not contain a number.");
            }

            string prefix = input.Substring(0, numberStartIndex);
            string numberPart = input.Substring(numberStartIndex);
            if (int.TryParse(numberPart, out int number))
            {

                number++;
                string newNumberPart = number.ToString(new string('0', numberPart.Length));
                return prefix + newNumberPart;
            }
            else
            {
                throw new ArgumentException("Numeric part of the input string is not valid.");
            }
        }

        public async Task<(bool, string)> InsertProduct(ProductMasterDTOModel productMasterDTOModel)
        {
            var insertProductMaster = "INSERT INTO tb_product_master(product_master_id,product_code,brand_id,sleeve,product_type,material) " +
                "VALUES(@productMasterUUID,@product_code,@brand_id,@sleeve,@product_type,@material); " +
                " SELECT @productMasterUUID AS LastInsertedId;";

            var insertProductDetails = "INSERT INTO tb_product_details(product_details_id,product_master_id,available_quantity,rate,discount,size_range) " +
                "VALUES(@productDetailsUUID,@product_master_id,@available_quantity,@rate,@discount,@size_range); " +
                "SELECT @productDetailsUUID;";
            var last_inserted_Code = "SELECT product_code FROM tb_product_master ORDER BY inserted_date DESC LIMIT 1;";
            var deleteProductMasterById = "delete from tb_product_master where product_master_id=@product_master_id";
            var deleteProductDetailById = "delete from product_deatils_id where product_master_id=@product_master_id";

            using (var connection = _dapperContext.CreateConnection())
            {
                var brandId = "";
                if (Guid.TryParse(productMasterDTOModel.brand_id, out _))
                {
                    brandId = productMasterDTOModel.brand_id;
                }
                else
                {
                    var (brand_exist_user_id, brand_message) = await _brandRepo.IsBrandExist(productMasterDTOModel.brand_id);
                    if (brand_exist_user_id != null)
                    {
                        brandId = brand_exist_user_id;
                    }
                    brandId = await _brandRepo.InsertBrand(productMasterDTOModel.brand_id);
                }
                var productCode = await connection.ExecuteScalarAsync<string>(last_inserted_Code);
                var productMasterParameters = new DynamicParameters();
                var productMasterUUID = Guid.NewGuid().ToString();
                // Generate the UUID in C#
                string product_code = IncrementNumber(productCode);
                productMasterParameters.Add("productMasterUUID", productMasterUUID);
                productMasterParameters.Add("product_code", product_code);
                productMasterParameters.Add("brand_id", brandId);
                productMasterParameters.Add("sleeve", productMasterDTOModel.sleeve);
                productMasterParameters.Add("product_type", productMasterDTOModel.product_type);
                productMasterParameters.Add("material", productMasterDTOModel.material);
                var product_master_id = await connection.ExecuteScalarAsync<string>(insertProductMaster, productMasterParameters);
                if (!string.IsNullOrEmpty(product_master_id))
                {

                    if (productMasterDTOModel.ProductDetailsListl.Count() > 0)
                    {
                        foreach (var productDeatils in productMasterDTOModel.ProductDetailsListl)
                        {
                            var productDetailsParameters = new DynamicParameters();
                            var productDetailsUUID = Guid.NewGuid().ToString(); // Generate the UUID in C# Generate the UUID in C#
                            productDetailsParameters.Add("productDetailsUUID", productDetailsUUID);
                            productDetailsParameters.Add("product_master_id", product_master_id);
                            productDetailsParameters.Add("available_quantity", productDeatils.available_quantity);
                            productDetailsParameters.Add("rate", productDeatils.rate);
                            productDetailsParameters.Add("discount", productDeatils.discount);
                            productDetailsParameters.Add("size_range", productDeatils.size_range);
                            var product_detail_id = await connection.ExecuteScalarAsync<string>(insertProductDetails, productDetailsParameters);
                            if (!string.IsNullOrEmpty(product_master_id))
                            {
                                continue;

                            }
                            else
                            {
                                await connection.ExecuteAsync(deleteProductDetailById, new { product_master_id });
                                await connection.ExecuteAsync(deleteProductMasterById, new { product_master_id });
                                return (false, StatusUtils.FAILED);
                            }
                        }

                    }
                    else
                    {
                        await connection.ExecuteAsync(deleteProductMasterById, new { product_master_id });
                        return (false, StatusUtils.FAILED);
                    }

                }
                return (true, product_master_id);
            }
        }

        public async Task<int> UpdateProductDetail(ProductDetailsUpdateDTOModel productDetailsUpdateDTOModel, string product_details_id)
        {
            try
            {
                var product_detail_update_query = "update tb_product_details SET available_quantity=@available_quantity," +
                    "rate=@rate,discount=@discount,size_range=@size_range," +
                    "updated_date=NOW() where product_details_id=@product_details_id;" +
                    "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";
                using (var connection = _dapperContext.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("available_quantity", productDetailsUpdateDTOModel.available_quantity);
                    parameters.Add("rate", productDetailsUpdateDTOModel.rate);
                    parameters.Add("discount", productDetailsUpdateDTOModel.discount);
                    parameters.Add("size_range", productDetailsUpdateDTOModel.size_range);
                    parameters.Add("product_details_id", product_details_id);

                    var update_product = await connection.ExecuteScalarAsync<int>
                      (product_detail_update_query, parameters);

                    return update_product;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while update product details");
            }
        }

        public async Task<int> UpdateProductMaster(ProductMasterUpdateDTOModel productMasterUpdateDTOModel, string product_master_id)
        {
            try
            {
                var product_master_update_query = "update tb_product_master SET brand_id=@brand_id,material=@material," +
                    "sleeve=@sleeve,product_type=@product_type," +
                    "updated_date=NOW() where product_master_id=@product_master_id;" +
                    "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";
                using (var connection = _dapperContext.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("brand_id", productMasterUpdateDTOModel.brand_id);
                    parameters.Add("sleeve", productMasterUpdateDTOModel.sleeve);
                    parameters.Add("product_type", productMasterUpdateDTOModel.product_type);
                    parameters.Add("product_master_id", product_master_id);

                    var update_product = await connection.ExecuteScalarAsync<int>
                      (product_master_update_query, parameters);

                    return update_product;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while update product details");
            }
        }
        public async Task<(bool, string)> DeleteProductDetail(string product_details_id, long action)
        {
            try
            {
                bool execute = false;
                var deleteQuery = $"UPDATE tb_product_details SET updated_date =NOW(), ";

                if (action == 0)
                {
                    execute = true;
                    deleteQuery += "is_active = 0 WHERE product_details_id = @product_details_id;";
                }
                else if (action == 1)
                {
                    execute = true;
                    deleteQuery += "is_active = 1 WHERE product_details_id = @product_details_id;";
                }
                else if (action == 2)
                {
                    execute = true;
                    deleteQuery += "is_delete = 1,is_active = 0 WHERE product_details_id = @product_details_id;";
                }
                if (execute)
                {
                    using (var connection = _dapperContext.CreateConnection())
                    {
                        deleteQuery += "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";
                        var parameters = new DynamicParameters();
                        parameters.Add("product_details_id", product_details_id);
                        var status = await connection.ExecuteAsync(deleteQuery, parameters);
                        if (status > 0)
                        {
                            if (action == 0)
                            {
                                return (true, StatusUtils.IS_ACTIVE_UPDATEDTION_SUCCESS);
                            }
                            else if (action == 1)
                            {
                                return (true, StatusUtils.IS_ACTIVE_UPDATEDTION_SUCCESS);
                            }
                            else if (action == 2)
                            {
                                return (true, StatusUtils.IS_DELETE_UPDATEDTION_SUCCESS);

                            }

                        }
                    }
                }
                return (false, StatusUtils.UPDATION_FAILED); ;

            }
            catch (Exception ex)
            {

                throw new Exception("Error occurred while deleting product details.", ex);
            }
        }

        public async Task<(bool, string)> DeleteProductMaster(string product_master_id, long action)
        {
            try
            {
                bool execute = false;
                var deleteQuery = $"UPDATE tb_product_master SET updated_date =NOW(), ";

                if (action == 0)
                {
                    execute = true;
                    deleteQuery += "is_active = 0 WHERE product_master_id = @product_master_id;";
                }
                else if (action == 1)
                {
                    execute = true;
                    deleteQuery += "is_active = 1 WHERE product_master_id = @product_master_id;";
                }
                else if (action == 2)
                {
                    execute = true;
                    deleteQuery += "is_delete = 1,is_active = 0 WHERE product_master_id = @product_master_id;";
                }
                if (execute)
                {
                    using (var connection = _dapperContext.CreateConnection())
                    {
                        deleteQuery += "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";
                        var parameters = new DynamicParameters();
                        parameters.Add("product_master_id", product_master_id);
                        var status = await connection.ExecuteAsync(deleteQuery, parameters);
                        if (status > 0)
                        {
                            if (action == 0)
                            {
                                return (true, StatusUtils.IS_ACTIVE_UPDATEDTION_SUCCESS);
                            }
                            else if (action == 1)
                            {
                                return (true, StatusUtils.IS_ACTIVE_UPDATEDTION_SUCCESS);
                            }
                            else if (action == 2)
                            {
                                return (true, StatusUtils.IS_DELETE_UPDATEDTION_SUCCESS);
                            }
                        }
                    }
                }
                return (false, StatusUtils.UPDATION_FAILED); 
            }
            catch (Exception ex)
            {

                throw new Exception("Error occurred while deleting product details.", ex);
            }
        }

        public async Task<IEnumerable<GetProductMasterModel>> GetProductMaster()
        {
            try
            {
                var company_master_query = "select tb_product_master.product_code,tb_product_master.product_master_id,tb_product_master.brand_id,tb_product_master.sleeve," +
                    "tb_product_master.material,tb_product_master.product_type,tb_product_master.is_active,tb_brand.brand_name from tb_product_master " +
                    "inner join tb_brand on tb_brand.brand_id=tb_product_master.brand_id " +
                    "where tb_product_master.is_delete = 0;";
                using (var connection = _dapperContext.CreateConnection())
                {
                    var company_master_list = await connection.QueryAsync<GetProductMasterModel>(company_master_query);
                    return company_master_list;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while retrieve product details");
            }
        }

        public async Task<GetProductDetailsByMasterId> GetProductDetailByMasterId(string product_master_id)
        {
            try
            {
                var query = @"
                            SELECT tb_product_master.product_code, tb_product_master.product_master_id, tb_product_master.brand_id, 
                                   tb_product_master.sleeve, tb_product_master.material, tb_product_master.product_type, 
                                   tb_product_master.is_active, tb_brand.brand_name 
                            FROM tb_product_master 
                            INNER JOIN tb_brand ON tb_brand.brand_id = tb_product_master.brand_id 
                            WHERE tb_product_master.product_master_id = @product_master_id AND tb_product_master.is_delete = 0;
    
                            SELECT * FROM tb_product_details 
                            WHERE product_master_id = @product_master_id AND is_delete = 0";

                using (var connection = _dapperContext.CreateConnection())
                using (var multi = await connection.QueryMultipleAsync(query, new { product_master_id }))
                {
                    var company = await multi.ReadSingleOrDefaultAsync<GetProductDetailsByMasterId>();
                    if (company != null)
                        company.ProductDetailsList = (await multi.ReadAsync<GetProductDetailsModel>()).ToList();
                    return company;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while retrieve product details");
            }
        }

    }
}
