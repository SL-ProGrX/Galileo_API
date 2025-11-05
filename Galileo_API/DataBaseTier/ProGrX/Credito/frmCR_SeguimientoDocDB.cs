using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Credito;
using PgxAPI.Models.ProGrX.Fondos;
using System.Diagnostics.Contracts;

namespace PgxAPI.DataBaseTier.ProGrX.Credito
{
    public class frmCR_SeguimientoDocDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 3; // Modulo de Créditos

        public frmCR_SeguimientoDocDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Método para aplicar la verificación del documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="documento"></param>
        /// <returns></returns>
        public ErrorDTO CR_SeguimientoDoc_Aplicar(int CodEmpresa, FrmCRSeguimientoDocData documento )
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok",
            };
            try
            {
                if(documento.documento == 0 || documento.verificacion == 0)
                {
                    response.Code = -1;
                    response.Description = "No se ha especificado el número del documento...";
                    return response;
                }

                if(documento.documento.ToString().Trim() != documento.verificacion.ToString().Trim())
                {
                    response.Code = -1;
                    response.Description = "El número del documento no concuerda con su verificación...";
                    return response;
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@" select isnull(count(*),0) as Existe from Tes_Transacciones where ndocumento = @documento
                      and Tipo = 'CK' and id_banco in(select cod_banco from reg_creditos where id_solicitud = @verificacion  )";

                    var existe = connection.Query<int>(query, new
                    {
                        documento = documento.documento,
                        verificacion = documento.verificacion
                    }).FirstOrDefault();

                    if(existe > 0)
                    {
                        response.Code = -1;
                        response.Description = "El documento especificado ya existe registrado en Tesorería...";
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

    }
}
