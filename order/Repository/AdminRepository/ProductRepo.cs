using Dapper;
using Microsoft.IdentityModel.Tokens;
using order.Context;
using order.DTOModel;
using order.IRepository.IAdminRepositorys;
using order.Models;
using order.Utils;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
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
            var deleteProductDetailById = "delete from tb_product_details where product_master_id=@product_master_id";

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
                    parameters.Add("material ", productMasterUpdateDTOModel.material);

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
                    var companyMasterList = await connection.QueryAsync<GetProductMasterModel>(company_master_query);

                    var companyMasterDetails = companyMasterList.Select(companyMaster => new GetProductMasterModel
                    {
                        product_master_id = companyMaster.product_master_id != null ? SecurityUtils.EncryptString(companyMaster.product_master_id) : null,
                        brand_id = companyMaster.brand_id != null ? SecurityUtils.EncryptString(companyMaster.brand_id) : null,
                        product_code = companyMaster.product_code,
                        sleeve = companyMaster.sleeve,
                        material = companyMaster.material,
                        product_type = companyMaster.product_type,
                        brand_name = companyMaster.brand_name,
                        is_active = companyMaster.is_active,
                    }).ToList();
                    return companyMasterDetails;
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
                    var product = await multi.ReadSingleOrDefaultAsync<GetProductDetailsByMasterId>();
                    if (product != null)
                        product.ProductDetailsList = (await multi.ReadAsync<GetProductDetailsModel>()).ToList();

                    product.product_master_id = product.product_master_id != null ? SecurityUtils.EncryptString(product.product_master_id) : null;
                    product.brand_id = product.brand_id != null ? SecurityUtils.EncryptString(product.brand_id) : null;


                    List<GetProductDetailsModel> ProductDetailsList= product.ProductDetailsList;


                    var ProductDetails = ProductDetailsList.Select(productDetail => new GetProductDetailsModel
                    {
                        product_details_id = productDetail.product_details_id != null ? SecurityUtils.EncryptString(productDetail.product_details_id) : null,
                        available_quantity = productDetail.available_quantity,
                        rate = productDetail.rate,
                        discount = productDetail.discount,
                        size_range = productDetail.size_range,
                        is_active = productDetail.is_active,
                    }).ToList();

                    product.ProductDetailsList= ProductDetails;

                    return product;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while retrieve product details");
            }
        }

        public async Task<(bool, string)> UpdateProduct(ProductMasterUpdateDtoModel productMasterUpdateDTOModel, string product_master_id)
        {
            var insertProductDetails = "INSERT INTO tb_product_details(product_details_id,product_master_id,available_quantity,rate,discount,size_range) " +
              "VALUES(@productDetailsUUID,@product_master_id,@available_quantity,@rate,@discount,@size_range); " +
              "SELECT @productDetailsUUID;";

            var product_detail_update_query = "update tb_product_details SET available_quantity=@available_quantity,"+
                "rate=@rate,discount=@discount,size_range=@size_range,is_active=1,is_delete=0,"+
                "updated_date=NOW() where product_details_id=@product_details_id;"+
                "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";

            var product_master_update_query = "update tb_product_master SET brand_id=@brand_id,material=@material,"+
                "sleeve=@sleeve,product_type=@product_type,updated_date=NOW() where product_master_id=@product_master_id;"+
                "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";



            var deleteProductDetailById = "update tb_product_details set is_active=@is_active,is_delete=@is_delete where product_master_id=@product_master_id";
            var deleteDetailById = "delete from tb_product_details where updated_date IS  NULL";


            using (var connection = _dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("brand_id", productMasterUpdateDTOModel.brand_id);
                parameters.Add("sleeve", productMasterUpdateDTOModel.sleeve);
                parameters.Add("product_type", productMasterUpdateDTOModel.product_type);
                parameters.Add("product_master_id", product_master_id);
                parameters.Add("material ", productMasterUpdateDTOModel.material);

                var update_product_master = await connection.ExecuteScalarAsync<int>(product_master_update_query, parameters);
                if (update_product_master == 1)
                {
                    await connection.ExecuteAsync(deleteProductDetailById, new { product_master_id, is_active =0, is_delete =1});

                    if (productMasterUpdateDTOModel.ProductDetailsListl.Count() > 0)
                    {
                        foreach (var productDeatils in productMasterUpdateDTOModel.ProductDetailsListl)
                        {
                            var productDetailsParameters = new DynamicParameters();
                            productDetailsParameters.Add("available_quantity", productDeatils.available_quantity);
                            productDetailsParameters.Add("rate", productDeatils.rate);
                            productDetailsParameters.Add("discount", productDeatils.discount);
                            productDetailsParameters.Add("size_range", productDeatils.size_range);
                            if (!string.IsNullOrEmpty(productDeatils.product_details_id))
                            {
                                productDeatils.product_details_id= SecurityUtils.DecryptString(productDeatils.product_details_id);

                                productDetailsParameters.Add("product_details_id", productDeatils.product_details_id);
                                var update_product_details = await connection.ExecuteScalarAsync<int>
                                    (product_detail_update_query, productDetailsParameters);
                                if(update_product_details==1)
                                {
                                    continue;
                                }
                                else
                                {
                                     await connection.ExecuteScalarAsync<int>
                                    (deleteDetailById);
                                    await connection.ExecuteAsync(deleteProductDetailById, new { product_master_id, is_active = 1, is_delete = 0 });
                                    return  (false, StatusUtils.FAILED);
                                }

                            }
                            else
                            {
                                var productDetailsUUID = Guid.NewGuid().ToString();
                                productDetailsParameters.Add("productDetailsUUID", productDetailsUUID);
                                var product_detail_id = await connection.ExecuteScalarAsync<string>(insertProductDetails, productDetailsParameters);
                                if (!string.IsNullOrEmpty(product_master_id))
                                {
                                    continue;

                                }
                                else
                                {
                                    await connection.ExecuteScalarAsync<int>
                                   (deleteDetailById);
                                    await connection.ExecuteAsync(deleteProductDetailById, new { product_master_id, is_active = 1, is_delete = 0 });
                                    return (false, StatusUtils.FAILED);
                                }
                            }
                            
                        }

                    }
                }
                else
                {
                    return (false, StatusUtils.FAILED);
                }
                return (false, StatusUtils.FAILED);
            }
        }
    }
}
