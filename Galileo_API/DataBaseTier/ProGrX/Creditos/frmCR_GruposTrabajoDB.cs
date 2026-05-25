using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrGruposTrabajoDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private const int VModulo = 3;
        private const string GuardadoExitoso = "Informacion guardada satisfactoriamente...";

        public FrmCrGruposTrabajoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista de grupos para los tabs de miembros, etiquetas y comités.
        /// </summary>
        public ErrorDto<List<CrGrupoTrabajoGrupoComboData>> CR_GruposTrabajo_GruposCombo_Obtener(int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(cod_grupo) as cod_grupo,
                    rtrim(isnull(descripcion, '')) as descripcion
                from crd_grupos
                order by cod_grupo;";

            return DbHelper.ExecuteListQuery<CrGrupoTrabajoGrupoComboData>(_portalDb, codEmpresa, sql);
        }

        private static string CR_GruposTrabajo_NormalizarTexto(string? valor)
            => (valor ?? string.Empty).Trim();

        private static ErrorDto? CR_GruposTrabajo_ValidarRequerido(string valor, string mensaje)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? DbHelper.ErrorResponse(mensaje)
                : null;
        }

        private static ErrorDto? CR_GruposTrabajo_ValidarGrupoExiste(SqlConnection conn, string codGrupo)
        {
            const string sql = @"
                select isnull(count(*), 0)
                from crd_grupos
                where cod_grupo = @cod_grupo;";

            var existe = conn.ExecuteScalar<int>(sql, new { cod_grupo = codGrupo });

            return existe > 0
                ? null
                : DbHelper.ErrorResponse("El grupo indicado no existe.");
        }



        private void CR_GruposTrabajo_RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = VModulo
            });
        }
    }
}
