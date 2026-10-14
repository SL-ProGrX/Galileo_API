using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;

namespace Galileo_API.BusinessLogic.ProGrX.Convenios
{
    public class FrmCrConveniosLiqDetalleBl
    {
        private readonly FrmCrConveniosLiqDetalleDb _db;
        private const string MsgSinOrden = "No se ha indicado el convenio y la orden a consultar.";
        private const string InformesValidos = "RDI";

        public FrmCrConveniosLiqDetalleBl(IConfiguration config)
        {
            _db = new FrmCrConveniosLiqDetalleDb(config);
        }

        public ErrorDto<CrLiqDetalleCabecera?> CrConveniosLiqDetalle_Cabecera_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden)
        {
            if (!OrdenValida(codConvenio, codOrden))
                return DbHelper.CreateErrorResponse<CrLiqDetalleCabecera?>(MsgSinOrden, -2, null);

            return _db.CrConveniosLiqDetalle_Cabecera_Obtener(codEmpresa, codConvenio, codOrden);
        }

        public ErrorDto<List<CrLiqDetalleInstitucion>> CrConveniosLiqDetalle_Instituciones_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden,
            string tipo)
        {
            if (!OrdenValida(codConvenio, codOrden))
                return DbHelper.CreateErrorResponse(MsgSinOrden, -2, new List<CrLiqDetalleInstitucion>());

            return _db.CrConveniosLiqDetalle_Instituciones_Obtener(codEmpresa, codConvenio, codOrden, tipo);
        }

        public ErrorDto<CrLiqDetalleConsulta?> CrConveniosLiqDetalle_Consulta_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden,
            string tipo,
            string informe,
            int? codInstitucion)
        {
            if (!OrdenValida(codConvenio, codOrden))
                return DbHelper.CreateErrorResponse<CrLiqDetalleConsulta?>(MsgSinOrden, -2, null);

            var tipoInforme = NormalizarInforme(informe);
            if (tipoInforme == null)
                return DbHelper.CreateErrorResponse<CrLiqDetalleConsulta?>(
                    "El tipo de informe no es válido.", -2, null);

            return _db.CrConveniosLiqDetalle_Consulta_Obtener(
                codEmpresa, codConvenio, codOrden, tipo, tipoInforme.Value, codInstitucion);
        }

        public ErrorDto<CrLiqDetalleArchivo?> CrConveniosLiqDetalle_Archivo_Generar(
            int codEmpresa,
            string codConvenio,
            int codOrden,
            string tipo,
            int? codInstitucion,
            string? nombreInstitucion)
        {
            if (!OrdenValida(codConvenio, codOrden))
                return DbHelper.CreateErrorResponse<CrLiqDetalleArchivo?>(MsgSinOrden, -2, null);

            return _db.CrConveniosLiqDetalle_Archivo_Generar(
                codEmpresa, codConvenio, codOrden, tipo, codInstitucion, nombreInstitucion);
        }

        private static bool OrdenValida(string codConvenio, int codOrden)
            => !string.IsNullOrWhiteSpace(codConvenio) && codOrden > 0;

        /// <summary>Primera letra del informe (Mid(cboInforme.Text, 1, 1) del VB6).</summary>
        private static char? NormalizarInforme(string? informe)
        {
            var letra = (informe ?? string.Empty).Trim().ToUpperInvariant();
            if (letra.Length == 0 || !InformesValidos.Contains(letra[0]))
                return null;

            return letra[0];
        }
    }
}
