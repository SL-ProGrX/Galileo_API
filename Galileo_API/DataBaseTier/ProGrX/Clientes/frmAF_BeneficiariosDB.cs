using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_BeneficiariosDB
    {
        private readonly IConfiguration _config;

        public frmAF_BeneficiariosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtener beneficiarios por cedula
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDTO<List<PersonaBeneficiarioDTO>> AF_PersonaBeneficiarios_Consulta(int CodEmpresa, string cedula, int? lineaId)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<PersonaBeneficiarioDTO>> { Code = 0, Result = new() };
            try
            {
                if (lineaId == null)
                {
                    lineaId = 0;
                }
                using var connection = new SqlConnection(stringConn);

                string sql = "exec spAFI_PERSONA_BENEFICIARIOS_Consulta @Cedula, @LineaId";
                response.Result = connection.Query<PersonaBeneficiarioDTO>(sql, new
                {
                    cedula,
                    lineaId
                }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// SP para Registrar, Actualizar y Eliminar beneficiario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDTO<int> AF_PersonaBeneficiarios_Registro(int CodEmpresa, PersonaBeneficiarioDTO dto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<int> { Code = 0 };
            try
            {
                using var connection = new SqlConnection(stringConn);

                string sql = @"exec spAFI_PERSONA_BENEFICIARIOS_Registra
                        @Cedula,
                        @Linea_Id,
                        @Cedula_Beneficiario,
                        @Nombre,
                        @Fecha_Nac,
                        @Tipo_Relacion,
                        @Cod_Parentesco,
                        @Porcentaje,
                        @AplicaSeguros,
                        @Notas,
                        @Direccion,
                        @Apto_Postal,
                        @Telefono1,
                        @Telefono2,
                        @Email,
                        @TipoMov,
                        @Registro_Usuario,
                        @Albacea,
                        @Albacea_Cedula,
                        @Albacea_Nombre,
                        @Albacea_Movil,
                        @Albacea_TelTra,
                        @Albacea_TelTra_Ext,
                        @Tipo_Id_R";

                var result = connection.QueryFirstOrDefault<dynamic>(sql, new
                {
                    dto.Cedula,
                    dto.Linea_Id,
                    dto.Cedula_Beneficiario,
                    dto.Nombre,
                    dto.Fecha_Nac,
                    dto.Tipo_Relacion,
                    dto.Cod_Parentesco,
                    dto.Porcentaje,
                    AplicaSeguros = dto.Aplica_Seguros ? 1 : 0,
                    dto.Notas,
                    dto.Direccion,
                    dto.Apto_Postal,
                    dto.Telefono1,
                    dto.Telefono2,
                    dto.Email,
                    dto.TipoMov,
                    dto.Registro_Usuario,
                    Albacea = dto.Albacea_Check ? 1 : 0,
                    dto.Albacea_Cedula,
                    dto.Albacea_Nombre,
                    dto.Albacea_Movil,
                    dto.Albacea_TelTra,
                    dto.Albacea_TelTra_Ext,
                    dto.Tipo_Id_R
                });

                if (result != null && result.LineaId != null)
                {
                    response.Result = (int)result.LineaId;
                    response.Description = "Guardado correctamente";
                }
                else
                {
                    response.Code = -1;
                    response.Description = "No se obtuvo respuesta del SP";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtener catalogos para beneficiarios
        /// Lista de tipos de identificacion y parentescos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<Beneficiarios_CatalogoDTO> AF_Beneficiarios_Catalogos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<Beneficiarios_CatalogoDTO>
            {
                Code = 0,
                Description = "Ok",
                Result = new Beneficiarios_CatalogoDTO()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                string sqlTipoId = @"select TIPO_ID as item, rtrim(Descripcion) as descripcion from AFI_TIPOS_IDS
                    Where TIPO_PERSONERIA = 'F' order by Tipo_Id";

                string sqlParentesco = @"select rtrim(cod_Parentesco) as item, rtrim(Descripcion) as descripcion
                    from sys_Parentescos where activo = 1";

                response.Result.TiposIdentificacion = connection.Query<DropDownListaGenericaModel>(sqlTipoId).ToList();
                response.Result.Parentescos = connection.Query<DropDownListaGenericaModel>(sqlParentesco).ToList();
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