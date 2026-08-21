using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Conciliacion;
using Galileo_API.Models.ProGrX_Conciliacion;

namespace Galileo_API.BusinessLogic.ProGrX_Conciliacion
{
    public sealed class FrmRastreoMovOpBL
    {
        private readonly FrmRastreoMovOpDB _db;

        public FrmRastreoMovOpBL(IConfiguration config)
        {
            _db = new FrmRastreoMovOpDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> RastreoMovOp_Periodos_Obtener(
            int codEmpresa)
        {
            var respuesta = _db.RastreoMovOp_Periodos_Obtener(codEmpresa);

            if (respuesta.Code < 0)
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = respuesta.Code,
                    Description = respuesta.Description,
                    Result = null,
                };
            }

            var lista = (respuesta.Result ?? [])
                .Select(periodo => new DropDownListaGenericaModel
                {
                    item = periodo.Id_Per_Historico,
                    descripcion =
                        $"{periodo.Anio} - {MConciliacionDB.fxConvierteMES(periodo.Mes)}",
                })
                .ToList();

            return DbHelper.CreateOkResponse(lista);
        }

        public ErrorDto<List<RastreoMovOpSaldosData>> RastreoMovOp_Saldos_Obtener(
            int codEmpresa,
            RastreoMovOpSaldosRequest request)
        {
            if (request.Id_Per_Historico is null or <= 0)
            {
                return new ErrorDto<List<RastreoMovOpSaldosData>>
                {
                    Code = -1,
                    Description = "Debe indicar un periodo valido.",
                    Result = null,
                };
            }

            var periodo = _db.RastreoMovOp_Periodo_Obtener(
                codEmpresa,
                request.Id_Per_Historico.Value);

            if (periodo.Code < 0)
            {
                return new ErrorDto<List<RastreoMovOpSaldosData>>
                {
                    Code = periodo.Code,
                    Description = periodo.Description,
                    Result = null,
                };
            }

            if (periodo.Result is null)
            {
                return new ErrorDto<List<RastreoMovOpSaldosData>>
                {
                    Code = -1,
                    Description = "No se encontro el periodo seleccionado.",
                    Result = null,
                };
            }

            var lineas = request.Lineas.GetValueOrDefault(30000);
            if (lineas <= 0)
            {
                lineas = 30000;
            }

            return _db.RastreoMovOp_Saldos_Obtener(
                codEmpresa,
                periodo.Result.Anio,
                periodo.Result.Mes,
                lineas,
                request.Diferencias.GetValueOrDefault(false));
        }
    }
}
