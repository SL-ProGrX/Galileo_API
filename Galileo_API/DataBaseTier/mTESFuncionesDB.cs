using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.Controllers.WFCSinpe;
using Galileo_API.DataBaseTier;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Text;

namespace Galileo.DataBaseTier
{
    public partial class MTesFuncionesDb
    {
        private readonly PortalDB _portalDB;
        private readonly SeguridadPortalDb _seguridadPortal;
        private readonly MTesoreria mTesoreria;
        private readonly VerificadorCoreFactory _factory;
        public const string zero6Append = "000000";
        public const string zero12Append = "000000000000";
        public const string fechaFormat = "yyyy/MM/dd";
        public const string fechaFormat2 = "ddMMyyyy";

        public MTesFuncionesDb(IConfiguration config)
        {
            _seguridadPortal = new SeguridadPortalDb(config);
            mTesoreria = new MTesoreria(config);
            _factory = new VerificadorCoreFactory(config);
            _portalDB = new PortalDB(config);
        }

       
        private static string Trunc(string? value, int maxLen)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length > maxLen ? value.Substring(0, maxLen) : value;
        }

        private string GetParametro(int codEmpresa, string codigo)
          => mTesoreria.fxTesParametro(codEmpresa, codigo);


        #region ===== SINPE General =====

        public ErrorDto<object> SbTesBancoSinpeGeneralCore(
            int codEmpresa,
            TesEmisionDocFiltros filtro,
            List<TesTransaccionDto> transaccionesList,
            Func<long> resolveConsecutivo)
        {
            long bancoConsec = 0;
            resolveConsecutivo();
            if (filtro.docInicial > 0)
            {
                bancoConsec = filtro.docInicial!;
            } 
                

            if (!string.Equals(filtro.tipoDoc, "TS", StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.CreateOkResponse<object>(
                    JsonConvert.SerializeObject(new
                    {
                        bancoConsec = bancoConsec.ToString(CultureInfo.InvariantCulture),
                        extension = string.Empty,
                        contenido = string.Empty,
                        results = Array.Empty<ErrorDto>()
                    }, Formatting.Indented));
            }

            if (transaccionesList == null || transaccionesList.Count == 0)
            {
                return DbHelper.CreateOkResponse<object>(
                    JsonConvert.SerializeObject(new
                    {
                        bancoConsec = bancoConsec.ToString(CultureInfo.InvariantCulture),
                        extension = string.Empty,
                        contenido = string.Empty,
                        results = Array.Empty<ErrorDto>()
                    }, Formatting.Indented));
            }

            if (string.IsNullOrWhiteSpace(filtro.usuario))
                return DbHelper.CreateErrorResponse<object>("Usuario requerido para procesar SINPE.");

            try
            {
                var servicio = _factory.CrearServicio(codEmpresa, filtro.usuario);
                var results = new List<ErrorDto>(capacity: transaccionesList.Count);

                foreach (var trx in transaccionesList)
                    results.Add(EmitirSinpe(servicio, codEmpresa, filtro.usuario, trx));

                return DbHelper.CreateOkResponse<object>(
                    JsonConvert.SerializeObject(new
                    {
                        bancoConsec = bancoConsec.ToString(CultureInfo.InvariantCulture),
                        extension = string.Empty,
                        contenido = string.Empty,
                        results
                    }, Formatting.Indented));
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private ErrorDto EmitirSinpe(IWfcSinpe servicio, int codEmpresa, string usuario, TesTransaccionDto trx)
        {
            var now = DateTime.Now;

            switch (trx.tipo_girosinpe)
            {
                case "CD":
                    return servicio.fxTesEmisionSinpeCreditoDirecto(codEmpresa, trx.nsolicitud, now, usuario, 0, 0);
                case "TR":
                    return servicio.fxTesEmisionSinpeTiempoReal(codEmpresa, trx.nsolicitud, now, usuario, 0, 0);
            }

            return new ErrorDto
            {
                Code = -1,
                Description = "Emision No Valida."
            };
        }

        #endregion

        public ErrorDto<dynamic> vTesFormatos(SqlConnection conn, string pFormato)
        {
            const string qFormato = "select Procedimiento,Extension from vTes_Formatos where cod_formato = @formato";
            var formatoData = conn.QueryFirstOrDefault(qFormato, new { formato = pFormato });

            if (formatoData == null)
                return DbHelper.CreateErrorResponse<dynamic>("Formato no encontrado.");

            return DbHelper.CreateOkResponse<dynamic>(formatoData);
        }
    }
}
