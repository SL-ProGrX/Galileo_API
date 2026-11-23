using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaConfiguracionesDB
    {
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Prea_Configuraciones_AutorizadoresEstado_Usuarios_Dropdown_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            const string sql = @"select rtrim(NOMBRE) item, rtrim(DESCRIPCION) descripcion
                from USUARIOS where ESTADO = 'A' order by NOMBRE";
            try
            {
                return DbHelper.CreateOkResponse(conn.Query<DropDownListaGenericaModel>(sql).ToList());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message, -1, []);
            }
        }

        public ErrorDto<List<CrPreaConfiguracionesAutorizadorEstadoDto>> CR_Prea_Configuraciones_AutorizadoresEstado_Lista_Obtener(int CodEmpresa)
        {
            const string sql = @"select rtrim(USUARIO) usuario, cast(isnull(ACTIVO,0) as bit) activo,
                rtrim(isnull(REGISTRO_USUARIO,'')) registro_usuario, REGISTRO_FECHA registro_fecha,
                rtrim(isnull(MODIFICA_USUARIO,'')) modifica_usuario, MODIFICA_FECHA modifica_fecha
                from dbo.CRD_PREA_AUTORIZADORES_ESTADO with (nolock) order by USUARIO";
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                return DbHelper.CreateOkResponse(
                    conn.Query<CrPreaConfiguracionesAutorizadorEstadoDto>(sql, commandTimeout: 15).ToList());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CrPreaConfiguracionesAutorizadorEstadoDto>>(ex.Message, -1, []);
            }
        }

        public ErrorDto CR_Prea_Configuraciones_AutorizadoresEstado_Guardar(int CodEmpresa, string usuarioRegistro, CrPreaConfiguracionesAutorizadorEstadoGuardarRequest request)
        {
            if (request is null)
                return DbHelper.ErrorResponse("La solicitud es requerida.", -2);

            var usuario = (request.usuario ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.ErrorResponse("Debe indicar el usuario autorizador.", -2);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            const string sql = @"if exists(select 1 from CRD_PREA_AUTORIZADORES_ESTADO where USUARIO=@Usuario)
                update CRD_PREA_AUTORIZADORES_ESTADO set ACTIVO=@Activo, MODIFICA_USUARIO=@Audita,
                    MODIFICA_FECHA=dbo.MyGetdate() where USUARIO=@Usuario
                else insert into CRD_PREA_AUTORIZADORES_ESTADO
                    (USUARIO,ACTIVO,REGISTRO_USUARIO,REGISTRO_FECHA)
                    values(@Usuario,@Activo,@Audita,dbo.MyGetdate())";
            try
            {
                conn.Execute(sql, new { Usuario = usuario, request.activo, Audita = (usuarioRegistro ?? string.Empty).Trim() });
                return DbHelper.OkResponse("Autorizador actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        public ErrorDto CR_Prea_Configuraciones_AutorizadoresEstado_Eliminar(int CodEmpresa, string usuarioAutorizador)
        {
            var usuario = (usuarioAutorizador ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.ErrorResponse("Debe indicar el usuario autorizador.", -2);

            const string sql = @"delete from dbo.CRD_PREA_AUTORIZADORES_ESTADO
                where USUARIO = @Usuario";
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var afectados = conn.Execute(sql, new { Usuario = usuario }, commandTimeout: 15);
                return afectados > 0
                    ? DbHelper.OkResponse("Autorizador eliminado correctamente.")
                    : DbHelper.ErrorResponse("No se encontró el autorizador indicado.", -1);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }
    }
}
