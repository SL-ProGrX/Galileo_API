using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo.Models.TES;
using Newtonsoft.Json;

namespace Galileo_API.DataBaseTier
{
    public class FrmTesTePlanesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb DBBitacora;

        public FrmTesTePlanesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            DBBitacora = new MSecurityMainDb(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Hacer scroll entre los planes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="codPlan"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<TesBancoPlanesData> TES_Planes_Scroll(int CodEmpresa, int scrollCode, string codPlan, int banco)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = "select Top 1 * from TES_BANCO_PLANES_TE";
                if (scrollCode == 1)
                {
                    query += " WHERE id_banco = @banco AND cod_Plan > @codPlan ORDER BY cod_Plan ASC";
                }
                else
                {
                    query += " WHERE id_banco = @banco AND cod_Plan < @codPlan ORDER BY cod_Plan DESC";
                }

                var response = conn.Query<TesBancoPlanesData>(query,
                    new { banco = banco, codPlan = codPlan }).FirstOrDefault() ?? new TesBancoPlanesData();
                return DbHelper.CreateOkResponse<TesBancoPlanesData>(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesBancoPlanesData>(ex.Message);
            }
        }

        /// <summary>
        /// Obtener información de planes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <param name="codPlan"></param>
        /// <returns></returns>
		public ErrorDto<TesBancoPlanesData> TES_PlanesConsulta_Obtener(int CodEmpresa, int banco, string codPlan)
		{
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
			try
			{
                var query = @"exec spTes_Planes_Consulta @banco, @codPlan";
                var response = conn.QueryFirstOrDefault<TesBancoPlanesData>(query,
                    new { banco = banco, codPlan = codPlan }) ?? new TesBancoPlanesData();

                return DbHelper.CreateOkResponse<TesBancoPlanesData>(response);
            }
			catch (Exception ex)
			{
                return DbHelper.CreateErrorResponse<TesBancoPlanesData>(ex.Message);
			}
		}

        /// <summary>
        /// Obtener información de grupos bancarios
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
		public ErrorDto<TesBancosGruposData> TES_Planes_BancosGrupos_Obtener(int CodEmpresa, int banco)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = @"select B.ID_BANCO, B.COD_GRUPO, B.DESCRIPCION , B.DESC_CORTA
                        , Bg.DESCRIPCION as 'Banco_Desc', Bg.DESC_CORTA as 'Banco_Desc_Corta'
                        from TES_BANCOS B inner join TES_BANCOS_GRUPOS Bg on B.COD_GRUPO = Bg.COD_GRUPO 
                        Where B.ID_Banco = @banco";
                var response = conn.QueryFirstOrDefault<TesBancosGruposData>(query,
                    new { banco = banco }) ?? new TesBancosGruposData();

                return DbHelper.CreateOkResponse<TesBancosGruposData>(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesBancosGruposData>(ex.Message);
            }
        }

        /// <summary>
        /// Agregar o actualizar un plan
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="infoPlan"></param>
        /// <returns></returns>
        public ErrorDto TES_Planes_Guardar(int CodEmpresa, string infoPlan)
        {
            TesBancoPlanesData request = JsonConvert.DeserializeObject<TesBancoPlanesData>(infoPlan) ?? new TesBancoPlanesData();
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = @"exec spTes_Planes_Registro @banco, @codPlan, @consecId, 
                            @consecInt, @usuario, 'A'";
                conn.Execute(query,
                    new
                    {
                        banco = request.id_banco,
                        codPlan = request.cod_plan,
                        consecId = request.numero_te,
                        consecInt = request.numero_interno,
                        usuario = request.registro_usuario.ToUpper()
                    });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = request.registro_usuario.ToUpper(),
                    DetalleMovimiento = "Cta Id: " + request.id_banco + ", Plan: " + request.cod_plan +
                    ", Consec Id: " + request.numero_te + ", Consec Interno: " + request.numero_interno,
                    Movimiento = "REGISTRA - WEB",
                    Modulo = 9
                });

                return DbHelper.OkResponse("Plan Registrado Satisfactoriamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }


        /// <summary>
        /// Borrar un plan
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="infoPlan"></param>
        /// <returns></returns>
        public ErrorDto TES_Planes_Borrar(int CodEmpresa, string infoPlan)
        {
            TesBancoPlanesData request = JsonConvert.DeserializeObject<TesBancoPlanesData>(infoPlan) ?? new TesBancoPlanesData();
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var query = @"exec spTes_Planes_Registro @banco, @codPlan, @consecId, 
                            @consecInt, @usuario, 'E'";
               conn.Execute(query,
                    new
                    {
                        banco = request.id_banco,
                        codPlan = request.cod_plan,
                        consecId = request.numero_te,
                        consecInt = request.numero_interno,
                        usuario = request.registro_usuario.ToUpper()
                    });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = request.registro_usuario.ToUpper(),
                    DetalleMovimiento = "Cta Id: " + request.id_banco + ", Plan: " + request.cod_plan +
                    ", Consec Id: " + request.numero_te + ", Consec Interno: " + request.numero_interno,
                    Movimiento = "ELIMINA - WEB",
                    Modulo = 9
                });

                return DbHelper.OkResponse("Plan Eliminado Satisfactoriamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

    }
}
