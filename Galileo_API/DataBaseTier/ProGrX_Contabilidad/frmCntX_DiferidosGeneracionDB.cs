using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXDiferidosGeneracionDB
    {
        private const string MsgPeriodoCerrado = "El período actual se encuentra cerrado o no se ha creado, verifique...";

        private readonly PortalDB _portalDb;
        private readonly MCntXCalculosDb _mCalculos;

        public FrmCntXDiferidosGeneracionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mCalculos = new MCntXCalculosDb(config);
        }

        /// <summary>
        /// Consulta los diferidos pendientes por generar usando el SP spCntX_Diferido_Pendientes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de consulta.</param>
        /// <returns>Lista de diferidos pendientes.</returns>
        public ErrorDto<List<CntXDiferidoPendienteDto>> Diferidos_Pendientes_Lista(int codEmpresa, CntXDiferidoPendienteParams param)
        {
            var sql = "spCntX_Diferido_Pendientes";
            var parameters = new
            {
                Contabilidad = param.CodContabilidad,
                Anio = param.PeriodoAnio,
                Mes = param.PeriodoMes
            };

            // Quitar el parámetro commandType
            return DbHelper.ExecuteListQuery<CntXDiferidoPendienteDto>(
                _portalDb,
                codEmpresa,
                sql,
                parameters
            );
        }

        /// <summary>
        /// Ejecuta el SP spCntX_Diferido_Asiento para registrar el asiento de un diferido.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del asiento.</param>
        /// <returns>Resultado con TipoDoc y NumDoc.</returns>
        public ErrorDto<CntXDiferidoAsientoResult?> Diferido_Asiento(int codEmpresa, CntXDiferidoAsientoParams param)
        {
            if (!_mCalculos.FxCntX_PeriodoVerifica(codEmpresa, param.CodContabilidad, param.Anio, param.Mes))
            {
                return new ErrorDto<CntXDiferidoAsientoResult?>
                {
                    Code = -1,
                    Description = MsgPeriodoCerrado
                };
            }

            var sql = "spCntX_Diferido_Asiento";
            var parameters = new
            {
                Contabilidad = param.CodContabilidad,
                param.Plantilla,
                param.Diferido,
                param.Anio,
                param.Mes,
                param.Usuario
            };

            return DbHelper.ExecuteSingleQuery<CntXDiferidoAsientoResult>(
                _portalDb,codEmpresa, sql, default, parameters
            );
        }

    }
}
