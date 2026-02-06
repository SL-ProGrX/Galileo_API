namespace Galileo_API.Models.ProGrX.Cajas
{
    public class CajasCajaChicaServiciosDto
    {
        public int cod_servicio { get; set; } = 0;
        public string serviciodesc { get; set; } = string.Empty;
        public int cod_recaudador { get; set; } = 0;
        public string recaudadordesc { get; set; } = string.Empty;
    }

    public class CajasCajaChicaTipoCambioRsDto
    {
        public decimal tc_venta { get; set; }

        public decimal tc_compra { get; set; }
    }

    public class CajasCajaChicaSociosBusquedaRsDto
    {
        public string cedula { get; set; } = string.Empty;
        public string cedular { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CajasCajaChicaServiciosDatosRsDto
    {
        public string cod_concepto { get; set; } = string.Empty;

        public decimal mnt_bruto { get; set; }
        public decimal comision { get; set; }
        public decimal impuesto { get; set; }
        public decimal mnt_neto { get; set; }

        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;

        public string ef_cta { get; set; } = string.Empty;
        public string ef_codigo { get; set; } = string.Empty;
    }

    public class CajasCajaChicaAplicarDbRequestDto
    {
        public int codempresa { get; set; } = 0;

        // contexto caja (normalmente viene de sesión / backend)
        public string cod_caja { get; set; } = string.Empty; // ModuloCajas.mCaja
        public int cod_apertura { get; set; } = 0; //ModuloCajas.mApertura
        public string cod_oficina { get; set; } = string.Empty; //ModuloCajas.mOficina

        // documento (ya resuelto por BL)
        public string tipo_documento { get; set; } = string.Empty; //vTipoDoc : cboDocumento
        public string numdoc { get; set; } = string.Empty; // vNumDoc : fxDocumentoConsecutivo(vTipoDoc)
        public string documento_deposito { get; set; } = string.Empty; // vAseDocDeposito : 

        // cliente
        public string cedula { get; set; } = string.Empty; //txtCedula.Text
        public string nombre { get; set; } = string.Empty; //txtNombre.Text 

        // servicio
        public string cod_recaudador { get; set; } = string.Empty; //txtRecaudadorCod.Text
        public string cod_servicio { get; set; } = string.Empty; //txtServicioCod.Text

        // refs/detalle
        public string? nref { get; set; }     // txtNRef.Text  (Mid(...,1,30))
        public string? detalle { get; set; }  // txtDetalle.Text

        // dinero
        public decimal monto { get; set; }  = 0; //curMonto = CCur(txtMonto.Text)
        public string cod_divisa { get; set; } = string.Empty; //cboDivisaActual.ItemData(cboDivisaActual.ListIndex)
        public decimal tipo_cambio { get; set; } = 0; // pTipoCambio: fxCajasTipoCambio(cod_divisa)

        // contabilidad (gEnlace) + monto aplicado (monto * fxSys_Tipo_Cambio_Apl(tipoCambio))
        public int cod_contabilidad { get; set; } = 0;
        public decimal monto_aplicado { get; set; } = 0; //curMonto * fxSys_Tipo_Cambio_Apl(pTipoCambio)

        // seguridad/auditoría
        public string usuario { get; set; } = string.Empty; //glogon.Usuario
    }

    public class CajasCajaChicaAplicarDbResponseDto
    {
        public string tipo_documento { get; set; } = string.Empty;
        public string numdoc { get; set; } = string.Empty;
    }
}
