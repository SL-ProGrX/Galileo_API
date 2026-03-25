using System.Data;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;
using Galileo.Models.Security;
using Microsoft.Extensions.Configuration;

namespace Galileo.DataBaseTier
{
    public class FrmSifParametrosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityDb;
        private readonly int vModulo = 10; // Módulo Núcleo

        public FrmSifParametrosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityDb = new MSecurityMainDb(config);
        }

        public ErrorDto<List<SifParametrosDto>> obtener_ParametrosSistema(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                connection.Execute("spSIFParametros", commandType: CommandType.StoredProcedure);

                const string query = "SELECT cod_parametro, descripcion, valor FROM SIF_PARAMETROS ORDER BY cod_parametro;";
                return connection.Query<SifParametrosDto>(query).ToList();
            });
        }

        public ErrorDto Parametros_Actualizar(int CodEmpresa, string usuario, SifParametrosDto parametros)
        {
            const string sql = "UPDATE SIF_PARAMETROS SET valor = @valor WHERE cod_parametro = @codParametro";

            // 1) Actualiza parámetro
            var upd = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql, new
            {
                valor = parametros.valor,
                codParametro = parametros.cod_parametro
            });

            if ((upd.Code ?? -1) != 0)
                return upd;

            // 2) Bitácora
             string detalleBitacora = $"Parametro: {parametros.cod_parametro} - {parametros.valor}";
            var bit = _securityDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                Modulo = vModulo,
                Movimiento = detalleBitacora,
                Detalle = "Modifica - WEB",
                AppNombre = "Galileo_API"
            });

            if ((bit.Code ?? -1) != 0)
                return DbHelper.ErrorResponse("Error al registrar bitácora: " + (bit.Description ?? "Error al registrar bitácora"), bit.Code ?? -1);

            return DbHelper.OkResponse("Registro actualizado satisfactoriamente");
        }

    }
}