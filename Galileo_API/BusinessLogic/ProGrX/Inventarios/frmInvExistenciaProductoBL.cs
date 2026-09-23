using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Globalization;

namespace Galileo.BusinessLogic
{
    public class FrmInvExistenciaProductoBL
    {
        private const string FormatoFecha = "yyyy-MM-dd";

        private const string ErrorProductoRequerido =
            "Debe indicar el c&oacute;digo del producto.";

        private const string ErrorUsuarioRequerido =
            "El usuario es requerido.";

        private const string ErrorFechaCorteInvalida =
            "La fecha de corte debe utilizar el formato yyyy-MM-dd.";

        private readonly FrmInvExistenciaProductoDB _db;

        public FrmInvExistenciaProductoBL(IConfiguration config)
        {
            _db = new FrmInvExistenciaProductoDB(config);
        }

        public ErrorDto INV_ExistenciaProducto_FechaServidor_Obtener(int CodEmpresa)
        {
            return _db.INV_ExistenciaProducto_FechaServidor_Obtener(CodEmpresa);
        }

        public ErrorDto<InvExistenciaProductoResultadoDto> INV_ExistenciaProducto_Consultar(
            int CodEmpresa,
            InvExistenciaProductoConsulta consulta)
        {
            var error = INV_ExistenciaProducto_Consulta_Validar(consulta);

            if (error is not null)
            {
                return DbHelper.CreateErrorResponse(
                    error,
                    -2,
                    new InvExistenciaProductoResultadoDto());
            }

            return _db.INV_ExistenciaProducto_Consultar(CodEmpresa, consulta);
        }

        private static string? INV_ExistenciaProducto_Consulta_Validar(
            InvExistenciaProductoConsulta consulta)
        {
            if (string.IsNullOrWhiteSpace(consulta.cod_producto))
            {
                return ErrorProductoRequerido;
            }

            if (string.IsNullOrWhiteSpace(consulta.usuario))
            {
                return ErrorUsuarioRequerido;
            }

            var fechaValida = DateTime.TryParseExact(
                consulta.fecha_corte,
                FormatoFecha,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);

            return fechaValida ? null : ErrorFechaCorteInvalida;
        }
    }
}
