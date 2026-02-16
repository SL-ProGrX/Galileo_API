using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using System.Collections.Generic;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCContratosPagadoresDB
    {
        private readonly PortalDB _portalDb;

        public FrmCxCContratosPagadoresDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Consulta la lista de pagadores de contrato, con filtros opcionales y tipo de join según chkTodos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de consulta.</param>
        /// <returns>Lista de pagadores.</returns>
        public ErrorDto<List<CxcContratoPagadorDto>> CxcContratosPagadores_Lista(int codEmpresa, CxcContratoPagadorListaParams param)
        {
            // Base del query (constante)
            string baseQuery = @"
                select
                    Per.CEDULA,
                    Per.NOMBRE,
                    ISNULL(Cnt.registro_usuario,'') as Usuario,
                    ISNULL(Cnt.registro_fecha,null) as Fecha,
                    case when ISNULL(Cnt.cod_contrato, 'NoExiste') = 'NoExiste' then 0 else 1 end as Activo
                from CXC_PERSONAS Per
                {0}
                where Per.Rol_Pagador = 1
                {1}
                order by Per.nombre, Cnt.cod_contrato desc";

            // JOIN dinámico según chkTodos
            string joinClause = param.ChkTodos
                ? "inner join CXC_CONTRATOS_PAGADORES Cnt on Per.CEDULA = Cnt.Cedula and Cnt.cod_contrato = @Cod_Contrato"
                : "left join CXC_CONTRATOS_PAGADORES Cnt on Per.CEDULA = Cnt.Cedula and Cnt.cod_contrato = @Cod_Contrato";

            // Filtros opcionales
            var filtros = new List<string>();
            if (!string.IsNullOrWhiteSpace(param.Cedula))
                filtros.Add("and Per.cedula like @CedulaFiltro");
            if (!string.IsNullOrWhiteSpace(param.Nombre))
                filtros.Add("and Per.Nombre like @NombreFiltro");

            string whereExtras = filtros.Count > 0 ? "\n" + string.Join("\n", filtros) : "";

            // Query final
            string query = string.Format(baseQuery, joinClause, whereExtras);

            var dynParams = new DynamicParameters();
            dynParams.Add("Cod_Contrato", param.Cod_Contrato);
            if (!string.IsNullOrWhiteSpace(param.Cedula))
                dynParams.Add("CedulaFiltro", $"%{param.Cedula}%");
            if (!string.IsNullOrWhiteSpace(param.Nombre))
                dynParams.Add("NombreFiltro", $"%{param.Nombre}%");

            return DbHelper.ExecuteListQuery<CxcContratoPagadorDto>(_portalDb, codEmpresa, query, dynParams);
        }

        /// <summary>
        /// Inserta un pagador en CxC_Contratos_Pagadores.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del pagador.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcContratoPagador_Insertar(int codEmpresa, CxcContratoPagadorSaveParams param)
        {
            var sql = @"
                INSERT INTO CxC_Contratos_Pagadores
                (cod_contrato, cedula, registro_fecha, registro_usuario)
                VALUES
                (@Cod_Contrato, @Cedula, dbo.MyGetdate(), @Registro_Usuario)";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Elimina un pagador de CxC_Contratos_Pagadores.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de eliminación.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcContratoPagador_Eliminar(int codEmpresa, CxcContratoPagadorDeleteParams param)
        {
            var sql = @"
                DELETE FROM CxC_Contratos_Pagadores
                WHERE cod_contrato = @Cod_Contrato AND cedula = @Cedula";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }
    }
}
