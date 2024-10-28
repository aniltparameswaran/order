using Dapper;
using order.Context;
using order.IRepository.IAdminRepositorys;
using order.Models;
using order.Utils;

namespace order.Repository.AdminRepository
{
    public class BrandRepo : IBrandRepo
    {
        private readonly DapperContext _dapperContext;

        public BrandRepo(DapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }
        public async Task<(bool, string)> DeleteBrand(string brand_id, long action)
        {
            try
            {
                bool execute = false;
                var deleteQuery = $"UPDATE tb_brand SET updated_date =NOW(), ";

                if (action == 0)
                {
                    execute = true;
                    deleteQuery += "is_active = 0 WHERE brand_id = @brand_id;";
                }
                else if (action == 1)
                {
                    execute = true;
                    deleteQuery += "is_active = 1 WHERE brand_id = @brand_id;";
                }
                else if (action == 2)
                {
                    execute = true;
                    deleteQuery += "is_delete = 1,is_active = 0 WHERE brand_id = @brand_id;";
                }
                if (execute)
                {
                    using (var connection = _dapperContext.CreateConnection())
                    {
                        deleteQuery += "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";
                        var parameters = new DynamicParameters();
                        parameters.Add("brand_id", brand_id);
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

                throw new Exception("Error occurred while deleting Brand.", ex);
            }
        }

        public async Task<IEnumerable<GetBrand>> GetBrand()
        {
            try
            {
                var query = "select brand_id,brand_name,is_active from tb_brand where is_delete=0;";
                using (var connection = _dapperContext.CreateConnection())
                {
                    
                   
                    var brands = await connection.QueryAsync<GetBrand>(query);

                    var brandDetails = brands.Select(brand => new GetBrand
                    {
                        brand_id = brand.brand_id != null ? SecurityUtils.EncryptString(brand.brand_id) : null,
                        brand_name = brand.brand_name,
                        is_active = brand.is_active,
                    }).ToList();
                    return brandDetails.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while retrieve brand");
            }
        }

        public async Task<string> InsertBrand(string brand_name)
        {
            try
            {
                var last_inserted_id = "";
                var brand_insert_query = @"INSERT INTO tb_brand (brand_id, brand_name) 
                                        VALUES (@UUID, @brand_name);
                                        SELECT @UUID AS LastInsertedId;";
                using (var connection = _dapperContext.CreateConnection())
                {

                    var parameter = new DynamicParameters();
                    var uuid = Guid.NewGuid().ToString(); // Generate the UUID in C#
                    parameter.Add("UUID", uuid);
                    parameter.Add("brand_name", brand_name);


                    last_inserted_id = await connection.ExecuteScalarAsync<string>(brand_insert_query, parameter);

                    if (!string.IsNullOrEmpty(last_inserted_id))
                    {

                        return last_inserted_id;
                    }
                    return last_inserted_id;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while Brand insert");
            }
        }

        public async Task<(string, string)> IsBrandExist(string brand_name)
        {
            try
            {
                var query = "select brand_id from tb_brand where brand_name = @brand_name and is_delete=0;";
                using (var connection = _dapperContext.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("brand_name", brand_name);
                    var brand_id = await connection.QuerySingleOrDefaultAsync<string>(query, parameters);
                    if (brand_id != null)
                    {
                        return (brand_id, StatusUtils.ALREADY_EXIST);
                    }
                    return (null, StatusUtils.NOT_EXIST);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while brand name checking");
            }
        }

        public async Task<int> UpdateBrandName(string brand_name, string brand_id)
        {
            try
            {
                var brand_update_query = "update tb_brand SET brand_name=@brand_name,updated_date=NOW() where brand_id=@brand_id;" +
                    "SELECT CASE WHEN ROW_COUNT() > 0 THEN 1 ELSE 0 END;";
                using (var connection = _dapperContext.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("brand_name", brand_name);
                    parameters.Add("brand_id", brand_id);

                    var update_brand = await connection.ExecuteScalarAsync<int>
                      (brand_update_query, parameters);

                    return update_brand;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error occur while update user");
            }
        }

    }
}
