using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;
using System.Collections.Generic;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivProfesionalesDB
    {
        private readonly PortalDB _portalDb;

        public FrmVivProfesionalesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de contactos de vivienda, filtrando por tipo profesional y estado si aplica.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Parámetros de filtro (TipoProfesional, Estado).</param>
        /// <returns>Lista de contactos.</returns>
        public ErrorDto<List<VivContactoDto>> VivContactos_Lista(int codEmpresa, VivContactoFiltroParams filtro)
        {
            var sql = new StringBuilder(@"
                SELECT idContacto, Identificacion, Nombre
                FROM ViviendaContactos
                WHERE 1 = 1
            ");

            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(filtro.TipoProfesional))
            {
                sql.Append(" AND TipoProfesional = @TipoProfesional");
                parameters.Add("TipoProfesional", filtro.TipoProfesional);
            }
            if (!string.IsNullOrEmpty(filtro.Estado))
            {
                sql.Append(" AND Estado = @Estado");
                parameters.Add("Estado", filtro.Estado);
            }

            return DbHelper.ExecuteListQuery<VivContactoDto>(
                _portalDb,
                codEmpresa,
                sql.ToString(),
                parameters
            );
        }

        /// <summary>
        /// Obtiene la lista de tipos de identificación para dropdown.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de tipos de identificación.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> VivTiposId_Lista(int codEmpresa)
        {
            var sql = @"
                SELECT TIPO_ID AS item, RTRIM(Descripcion) AS descripcion
                FROM AFI_TIPOS_IDS
                ORDER BY Tipo_Id";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                null
            );
        }

        /// <summary>
        /// Ejecuta el SP spCrd_SGT_Bancos para obtener los bancos accesibles por usuario y divisa.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del SP (usuario, divisa).</param>
        /// <returns>Lista de bancos.</returns>
        public ErrorDto<List<CrdSgtBancoDto>> CrdSgtBancos_Lista(int codEmpresa, CrdSgtBancoParams param)
        {
            var sql = "spCrd_SGT_Bancos";
            var parameters = new
            {
                Usuario = param.Usuario,
                Divisa = param.Divisa
            };

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Query<CrdSgtBancoDto>(sql, parameters, commandType: System.Data.CommandType.StoredProcedure).AsList()
            );

            return new ErrorDto<List<CrdSgtBancoDto>>
            {
                Result = result.Result,
                Code = 0,
                Description = "Ok"
            };
        }

        /// <summary>
        /// Obtiene las cuentas bancarias de un usuario por identificación.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="identificacion">Identificación del usuario.</param>
        /// <returns>Lista de cuentas bancarias.</returns>
        public ErrorDto<List<VivCuentaBancariaDto>> VivCuentasBancarias_Lista(int codEmpresa, string identificacion)
        {
            var sql = @"
                select rtrim(B.Descripcion) as Banco,
                       case when C.tipo = 'A' then 'Ahorros' else 'Corriente' end as TipoDesc,
                       C.cod_Divisa,
                       C.CUENTA_INTERNA as Cuenta_Interna,
                       C.CUENTA_INTERBANCA as Cuenta_Interbanca,
                       C.ACTIVA,
                       C.DESTINO,
                       C.REGISTRO_FECHA,
                       C.REGISTRO_USUARIO
                from SYS_CUENTAS_BANCARIAS C
                inner join TES_BANCOS_GRUPOS B on C.cod_banco = B.cod_grupo
                where C.Identificacion = @Identificacion";

            return DbHelper.ExecuteListQuery<VivCuentaBancariaDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Identificacion = identificacion }
            );
        }

        /// <summary>
        /// Ejecuta el SP spCrdViv_Contacto_Consulta para obtener el detalle de un contacto.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="idContacto">Id del contacto.</param>
        /// <returns>Detalle del contacto.</returns>
        public ErrorDto<CrdVivContactoConsultaDto> CrdVivContacto_Consulta(int codEmpresa, int idContacto)
        {
            var sql = "spCrdViv_Contacto_Consulta";
            var parameters = new { IdContacto = idContacto };

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<CrdVivContactoConsultaDto>(sql, parameters, commandType: System.Data.CommandType.StoredProcedure)
            );

            return new ErrorDto<CrdVivContactoConsultaDto>
            {
                Result = result.Result,
                Code = result.Result != null ? 0 : -2,
                Description = result.Result != null ? "Ok" : "No se encontró el contacto."
            };
        }
    }
}
