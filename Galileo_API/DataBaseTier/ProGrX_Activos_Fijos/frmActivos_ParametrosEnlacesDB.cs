using Dapper;
using Galileo.Models.ERROR;


namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosParametrosEnlacesDB
    {
        private readonly PortalDB _portalDB;

        public FrmActivosParametrosEnlacesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método para guardar los parámetros de enlaces de proveedores de activos fijos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto Activos_ParametrosEnlaces_Proveedores_Guardar(int CodEmpresa)
        {
            var query = "";
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                query = $@"INSERT INTO Activos_proveedores(cod_proveedor,descripcion)
                                (select cod_proveedor,descripcio From cxp_proveedores 
                                    where cod_proveedor not in(select cod_proveedor from Activos_proveedores))";
                connection.Execute(query);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

    }
}