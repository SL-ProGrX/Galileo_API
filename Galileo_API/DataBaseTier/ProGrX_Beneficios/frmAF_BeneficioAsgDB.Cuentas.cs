using System.Data;
using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficioAsgDB
    {
        /// <summary>
        /// Lista de cuentas bancarias por identificación, banco y divisa (SP spSys_Cuentas_Bancarias).
        /// </summary>
        public ErrorDto<List<CuentaListaData>> Cuentas_Obtener(int CodCliente, string Identificacion, int BancoId, int DivisaCheck)
        {
            var identificacion = (Identificacion ?? string.Empty).Replace("undefined", "").Replace(" ", "").Trim();

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<CuentaListaData>(
                    "[spSys_Cuentas_Bancarias]",
                    new { Identificacion = identificacion, BancoId, DivisaCheck },
                    commandType: CommandType.StoredProcedure).ToList());

            return new ErrorDto<List<CuentaListaData>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<CuentaListaData>()
            };
        }

        /// <summary>
        /// Lista de cuentas bancarias asignadas al usuario.
        /// </summary>
        public ErrorDto<List<CuentaListaData>> CuentasUsuario_Obtener(int CodCliente, string usuario)
        {
            const string sql = @"SELECT B.id_banco AS 'IdX', RTRIM(B.descripcion) AS 'ItmX'
                                 FROM tes_banco_asg T
                                 INNER JOIN Tes_Bancos B ON T.id_banco = B.id_banco
                                 WHERE T.nombre = @usuario";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<CuentaListaData>(sql, new { usuario }).ToList());

            return new ErrorDto<List<CuentaListaData>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<CuentaListaData>()
            };
        }
    }
}
