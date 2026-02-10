namespace Galileo_API.Models.ProGrX.Cajas
{
    public class CajasRoeModelDto
    {
        // ---- socio ----
        public string? cedula { get; set; }
        public string? nombre { get; set; }

        public DateTime aso_fecha_nac { get; set; }
        public string aso_telefono { get; set; }

        public string aso_estado_persona_desc { get; set; }
        public string aso_estado_persona { get; set; }

        public string aso_institucion_desc { get; set; }
        public string aso_departamento_desc { get; set; }
        public string aso_seccion_desc { get; set; }
        public string aso_profesion_desc { get; set; }

        public string aso_provincia_desc { get; set; }
        public string aso_canton_desc { get; set; }
        public string aso_distrito_desc { get; set; }

        public string aso_tipo_id { get; set; }
        public string aso_direccion { get; set; }
        public string aso_tipoiddesc { get; set; }
        public string tipo_personeria { get; set; }

        public string aso_pais_desc { get; set; }
        public string aso_nacionalidad { get; set; }
        public string aso_estado_civil_desc { get; set; }
        public string aso_estado_laboral_desc { get; set; }
        public string aso_nivel_academico_desc { get; set; }

        // ---- roe.* (tabla cajas_roe) ----
        public int id_roe { get; set; }
        public int? id_sesion { get; set; }

        public string? cedula_aso { get; set; }

        public string? identificacion_depo { get; set; }
        public string? nombre_depo { get; set; }

        public string? provincia { get; set; }
        public string? canton { get; set; }
        public string? distrito { get; set; }

        public string? direccion { get; set; }
        public string? telefono_depo { get; set; }

        public DateTime? fecha_nac_const_empr { get; set; }

        public string? datos_beneficiario { get; set; }
        public string? num_doc { get; set; }

        public DateTime? fecha { get; set; }
        public string? hora { get; set; }

        public decimal? monto_local { get; set; }
        public decimal? monto_dol { get; set; }

        public string? origen_fondos { get; set; }

        public string? tipo_trans { get; set; }
        public string? tipo_operacion { get; set; }

        public string? estado { get; set; }

        public string? registro_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }

        public string? actualiza_usuario { get; set; }
        public DateTime? actualiza_fecha { get; set; }

        public string? imprime_usuario { get; set; }
        public DateTime? imprime_fecha { get; set; }

        public string? usuario_anulacion { get; set; }
        public DateTime? fecha_anulacion { get; set; }
        public string? observ_anulacion { get; set; }

        // ---- descripciones depositante ----
        public string dep_tipoiddesc { get; set; }
        public string dep_pais_desc { get; set; }
        public string dep_provincia_desc { get; set; }
        public string dep_canton_desc { get; set; }
        public string dep_distrito_desc { get; set; }
    }

    public class CajasRoeActualizaParamsModel
    {
        // @ROE
        public int roe { get; set; }

        // @TipoIdDesc
        public string tipoiddesc { get; set; } = "";

        // @Provincia, @Canton, @Distrito, @Direccion
        public string provincia { get; set; } = "";
        public string canton { get; set; } = "";
        public string distrito { get; set; } = "";
        public string direccion { get; set; } = "";

        // @Telefono, @FechaNac
        public string telefono { get; set; } = "";
        public DateTime fecha_nac { get; set; }

        // @TipoTrans, @TipoOperacion
        public string tipo_trans { get; set; } = "";
        public string tipo_operacion { get; set; } = "";

        // @OrigenRecursos, @Observaciones, @DatosBeneficiario
        public string origen_recursos { get; set; } = "";
        public string observaciones { get; set; } = "";
        public string datos_beneficiario { get; set; } = "";

        // @Usuario
        public string usuario { get; set; } = "";

        // @TipoId (nullable)
        public short? tipo_id { get; set; }

        // @PaisId, @Pais
        public string pais_id { get; set; } = "";
        public string pais { get; set; } = "";

        // @ProvinciaId, @CantonId, @DistritoId
        public string provincia_id { get; set; } = "";
        public string canton_id { get; set; } = "";
        public string distrito_id { get; set; } = "";
    }

    public class SpResultadoModel
    {
        public short pass { get; set; }
        public string mensaje { get; set; } = "";
    }

    public class CajasRoeImprimeParamsModel
    {
        public int roe { get; set; }
        public string usuario { get; set; } = "";
    }

}
