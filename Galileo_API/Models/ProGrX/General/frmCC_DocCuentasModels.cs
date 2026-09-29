namespace Galileo.Models.GEN
{
    /// <summary>
    /// Cuenta contable con máscara y descripción (Form_Load / txtCuenta_KeyDown / sbBuscaCuenta).
    /// </summary>
    public class CcDocCuentasCuentaData
    {
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Cuenta configurada del tipo de documento y bandera vVerificar de mRecibos.fxDocumentoCuenta.
    /// </summary>
    public class CcDocCuentasVerificarData
    {
        public string cuenta { get; set; } = string.Empty;
        public bool verificar { get; set; }
    }

    /// <summary>
    /// Datos capturados en frmCC_DocCuentas para validar el documento.
    /// </summary>
    public class CcDocCuentasValidarRequest
    {
        public string? cuenta { get; set; }
        public string? referencia { get; set; }
        public string? detalle { get; set; }
    }

    /// <summary>
    /// Resultado homologado a las globales vAseDocCuenta, vAseDocDeposito, vAseDocDetalle y vAseDocValido.
    /// </summary>
    public class CcDocCuentasResultadoData
    {
        public string cuenta { get; set; } = string.Empty;
        public string deposito { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public bool valido { get; set; }
    }
}
