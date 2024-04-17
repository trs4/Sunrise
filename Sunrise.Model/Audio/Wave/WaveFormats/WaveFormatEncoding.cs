namespace Sunrise.Model.Audio.Wave;

/// <summary>Summary description for WaveFormatEncoding</summary>
public enum WaveFormatEncoding : ushort
{
    /// <summary>WAVE_FORMAT_UNKNOWN,	Microsoft Corporation</summary>
    Unknown = 0x0000,

    /// <summary>WAVE_FORMAT_PCM		Microsoft Corporation</summary>
    Pcm = 0x0001,

    /// <summary>WAVE_FORMAT_ADPCM		Microsoft Corporation</summary>
    Adpcm = 0x0002,

    /// <summary>WAVE_FORMAT_IEEE_FLOAT Microsoft Corporation</summary>
    IeeeFloat = 0x0003,

    /// <summary>WAVE_FORMAT_VSELP		Compaq Computer Corp.</summary>
    Vselp = 0x0004,

    /// <summary>WAVE_FORMAT_IBM_CVSD	IBM Corporation</summary>
    IbmCvsd = 0x0005,

    /// <summary>WAVE_FORMAT_ALAW		Microsoft Corporation</summary>
    ALaw = 0x0006,

    /// <summary>WAVE_FORMAT_MULAW		Microsoft Corporation</summary>
    MuLaw = 0x0007,

    /// <summary>WAVE_FORMAT_DTS		Microsoft Corporation</summary>
    Dts = 0x0008,

    /// <summary>WAVE_FORMAT_DRM		Microsoft Corporation</summary>
    Drm = 0x0009,

    /// <summary>WAVE_FORMAT_WMAVOICE9 </summary>
    WmaVoice9 = 0x000A,

    /// <summary>WAVE_FORMAT_OKI_ADPCM	OKI</summary>
    OkiAdpcm = 0x0010,

    /// <summary>WAVE_FORMAT_DVI_ADPCM	Intel Corporation</summary>
    DviAdpcm = 0x0011,

    /// <summary>WAVE_FORMAT_IMA_ADPCM  Intel Corporation</summary>
    ImaAdpcm = DviAdpcm,

    /// <summary>WAVE_FORMAT_MEDIASPACE_ADPCM Videologic</summary>
    MediaspaceAdpcm = 0x0012,

    /// <summary>WAVE_FORMAT_SIERRA_ADPCM Sierra Semiconductor Corp </summary>
    SierraAdpcm = 0x0013,

    /// <summary>WAVE_FORMAT_G723_ADPCM Antex Electronics Corporation </summary>
    G723Adpcm = 0x0014,

    /// <summary>WAVE_FORMAT_DIGISTD DSP Solutions, Inc.</summary>
    DigiStd = 0x0015,

    /// <summary>WAVE_FORMAT_DIGIFIX DSP Solutions, Inc.</summary>
    DigiFix = 0x0016,

    /// <summary>WAVE_FORMAT_DIALOGIC_OKI_ADPCM Dialogic Corporation</summary>
    DialogicOkiAdpcm = 0x0017,

    /// <summary>WAVE_FORMAT_MEDIAVISION_ADPCM Media Vision, Inc.</summary>
    MediaVisionAdpcm = 0x0018,

    /// <summary>WAVE_FORMAT_CU_CODEC Hewlett-Packard Company </summary>
    CUCodec = 0x0019,

    /// <summary>WAVE_FORMAT_YAMAHA_ADPCM Yamaha Corporation of America</summary>
    YamahaAdpcm = 0x0020,

    /// <summary>WAVE_FORMAT_SONARC Speech Compression</summary>
    SonarC = 0x0021,

    /// <summary>WAVE_FORMAT_DSPGROUP_TRUESPEECH DSP Group, Inc </summary>
    DspGroupTrueSpeech = 0x0022,

    /// <summary>WAVE_FORMAT_ECHOSC1 Echo Speech Corporation</summary>
    EchoSpeechCorporation1 = 0x0023,

    /// <summary>WAVE_FORMAT_AUDIOFILE_AF36, Virtual Music, Inc.</summary>
    AudioFileAf36 = 0x0024,

    /// <summary>WAVE_FORMAT_APTX Audio Processing Technology</summary>
    Aptx = 0x0025,

    /// <summary>WAVE_FORMAT_AUDIOFILE_AF10, Virtual Music, Inc.</summary>
    AudioFileAf10 = 0x0026,

    /// <summary>WAVE_FORMAT_PROSODY_1612, Aculab plc</summary>
    Prosody1612 = 0x0027,

    /// <summary>WAVE_FORMAT_LRC, Merging Technologies S.A. </summary>
    Lrc = 0x0028,

    /// <summary>WAVE_FORMAT_DOLBY_AC2, Dolby Laboratories</summary>
    DolbyAc2 = 0x0030,

    /// <summary>WAVE_FORMAT_GSM610, Microsoft Corporation</summary>
    Gsm610 = 0x0031,

    /// <summary>WAVE_FORMAT_MSNAUDIO, Microsoft Corporation</summary>
    MsnAudio = 0x0032,

    /// <summary>WAVE_FORMAT_ANTEX_ADPCME, Antex Electronics Corporation</summary>
    AntexAdpcme = 0x0033,

    /// <summary>WAVE_FORMAT_CONTROL_RES_VQLPC, Control Resources Limited </summary>
    ControlResVqlpc = 0x0034,

    /// <summary>WAVE_FORMAT_DIGIREAL, DSP Solutions, Inc. </summary>
    DigiReal = 0x0035,

    /// <summary>WAVE_FORMAT_DIGIADPCM, DSP Solutions, Inc.</summary>
    DigiAdpcm = 0x0036,

    /// <summary>WAVE_FORMAT_CONTROL_RES_CR10, Control Resources Limited</summary>
    ControlResCr10 = 0x0037,


    /// <summary>Natural MicroSystems</summary>
    WAVE_FORMAT_NMS_VBXADPCM = 0x0038,

    /// <summary>Crystal Semiconductor IMA ADPCM</summary>
    WAVE_FORMAT_CS_IMAADPCM = 0x0039,

    /// <summary>Echo Speech Corporation</summary>
    WAVE_FORMAT_ECHOSC3 = 0x003A,

    /// <summary>Rockwell International</summary>
    WAVE_FORMAT_ROCKWELL_ADPCM = 0x003B,

    /// <summary>Rockwell International</summary>
    WAVE_FORMAT_ROCKWELL_DIGITALK = 0x003C,

    /// <summary>Xebec Multimedia Solutions Limited</summary>
    WAVE_FORMAT_XEBEC = 0x003D,

    /// <summary>Antex Electronics Corporation</summary>
    WAVE_FORMAT_G721_ADPCM = 0x0040,

    /// <summary>Antex Electronics Corporation</summary>
    WAVE_FORMAT_G728_CELP = 0x0041,

    /// <summary>Microsoft Corporation</summary>
    WAVE_FORMAT_MSG723 = 0x0042,

    /// <summary>WAVE_FORMAT_MPEG, Microsoft Corporation </summary>
    Mpeg = 0x0050,

    /// <summary>InSoft, Inc</summary>
    WAVE_FORMAT_RT24 = 0x0052,

    /// <summary>InSoft, Inc</summary>
    WAVE_FORMAT_PAC = 0x0053,

    /// <summary>WAVE_FORMAT_MPEGLAYER3, ISO/MPEG Layer3 Format Tag</summary>
    MpegLayer3 = 0x0055,

    /// <summary>Lucent Technologies</summary>
    WAVE_FORMAT_LUCENT_G723 = 0x0059,

    /// <summary>Cirrus Logic</summary>
    WAVE_FORMAT_CIRRUS = 0x0060,

    /// <summary>ESS Technology</summary>
    WAVE_FORMAT_ESPCM = 0x0061,

    /// <summary>Voxware Inc</summary>
    WAVE_FORMAT_VOXWARE = 0x0062,

    /// <summary>Canopus, co., Ltd</summary>
    WAVE_FORMAT_CANOPUS_ATRAC = 0x0063,

    /// <summary>APICOM</summary>
    WAVE_FORMAT_G726_ADPCM = 0x0064,

    /// <summary>APICOM</summary>
    WAVE_FORMAT_G722_ADPCM = 0x0065,

    /// <summary>Microsoft Corporation</summary>
    WAVE_FORMAT_DSAT_DISPLAY = 0x0067,

    /// <summary>Voxware Inc</summary>
    WAVE_FORMAT_VOXWARE_BYTE_ALIGNED = 0x0069,

    /// <summary>Voxware Inc</summary>
    WAVE_FORMAT_VOXWARE_AC8 = 0x0070,

    /// <summary>Voxware Inc</summary>
    WAVE_FORMAT_VOXWARE_AC10 = 0x0071,

    /// <summary>Voxware Inc</summary>
    WAVE_FORMAT_VOXWARE_AC16 = 0x0072,

    /// <summary>Voxware Inc</summary>
    WAVE_FORMAT_VOXWARE_AC20 = 0x0073,

    /// <summary>Voxware Inc</summary>
    WAVE_FORMAT_VOXWARE_RT24 = 0x0074,

    /// <summary>Voxware Inc</summary>
    WAVE_FORMAT_VOXWARE_RT29 = 0x0075,

    /// <summary>Voxware Inc</summary>
    WAVE_FORMAT_VOXWARE_RT29HW = 0x0076,

    /// <summary>Voxware Inc</summary>
    WAVE_FORMAT_VOXWARE_VR12 = 0x0077,

    /// <summary>Voxware Inc</summary>
    WAVE_FORMAT_VOXWARE_VR18 = 0x0078,

    /// <summary>Voxware Inc</summary>
    WAVE_FORMAT_VOXWARE_TQ40 = 0x0079,

    /// <summary>Softsound, Ltd</summary>
    WAVE_FORMAT_SOFTSOUND = 0x0080,

    /// <summary>Voxware Inc</summary>
    WAVE_FORMAT_VOXWARE_TQ60 = 0x0081,

    /// <summary>Microsoft Corporation</summary>
    WAVE_FORMAT_MSRT24 = 0x0082,

    /// <summary>AT&T Labs, Inc</summary>
    WAVE_FORMAT_G729A = 0x0083,

    /// <summary>Motion Pixels</summary>
    WAVE_FORMAT_MVI_MVI2 = 0x0084,

    /// <summary>DataFusion Systems (Pty) (Ltd)</summary>
    WAVE_FORMAT_DF_G726 = 0x0085,

    /// <summary>DataFusion Systems (Pty) (Ltd)</summary>
    WAVE_FORMAT_DF_GSM610 = 0x0086,

    /// <summary>Iterated Systems, Inc</summary>
    WAVE_FORMAT_ISIAUDIO = 0x0088,

    /// <summary>OnLive! Technologies, Inc</summary>
    WAVE_FORMAT_ONLIVE = 0x0089,

    /// <summary>Siemens Business Communications Sys</summary>
    WAVE_FORMAT_SBC24 = 0x0091,

    /// <summary>Sonic Foundry</summary>
    WAVE_FORMAT_DOLBY_AC3_SPDIF = 0x0092,

    /// <summary>MediaSonic</summary>
    WAVE_FORMAT_MEDIASONIC_G723 = 0x0093,

    /// <summary>Aculab plc</summary>
    WAVE_FORMAT_PROSODY_8KBPS = 0x0094,

    /// <summary>ZyXEL Communications, Inc</summary>
    WAVE_FORMAT_ZYXEL_ADPCM = 0x0097,

    /// <summary>Philips Speech Processing</summary>
    WAVE_FORMAT_PHILIPS_LPCBB = 0x0098,

    /// <summary>Studer Professional Audio AG</summary>
    WAVE_FORMAT_PACKED = 0x0099,

    /// <summary>Malden Electronics Ltd</summary>
    WAVE_FORMAT_MALDEN_PHONYTALK = 0x00A0,

    /// <summary>WAVE_FORMAT_GSM</summary>
    Gsm = 0x00A1,

    /// <summary>WAVE_FORMAT_G729</summary>
    G729 = 0x00A2,

    /// <summary>WAVE_FORMAT_G723</summary>
    G723 = 0x00A3,

    /// <summary>WAVE_FORMAT_ACELP</summary>
    Acelp = 0x00A4,

    /// <summary>WAVE_FORMAT_RAW_AAC1</summary>
    RawAac = 0x00FF,

    /// <summary>Rhetorex Inc</summary>
    WAVE_FORMAT_RHETOREX_ADPCM = 0x0100,

    /// <summary>BeCubed Software Inc</summary>
    WAVE_FORMAT_IRAT = 0x0101,

    /// <summary>Vivo Software</summary>
    WAVE_FORMAT_VIVO_G723 = 0x0111,

    /// <summary>Vivo Software</summary>
    WAVE_FORMAT_VIVO_SIREN = 0x0112,

    /// <summary>Digital Equipment Corporation</summary>
    WAVE_FORMAT_DIGITAL_G723 = 0x0123,

    /// <summary>Sanyo Electric Co., Ltd</summary>
    WAVE_FORMAT_SANYO_LD_ADPCM = 0x0125,

    /// <summary>Sipro Lab Telecom Inc</summary>
    WAVE_FORMAT_SIPROLAB_ACEPLNET = 0x0130,

    /// <summary>Sipro Lab Telecom Inc</summary>
    WAVE_FORMAT_SIPROLAB_ACELP4800 = 0x0131,

    /// <summary>Sipro Lab Telecom Inc</summary>
    WAVE_FORMAT_SIPROLAB_ACELP8V3 = 0x0132,

    /// <summary>Sipro Lab Telecom Inc</summary>
    WAVE_FORMAT_SIPROLAB_G729 = 0x0133,

    /// <summary>Sipro Lab Telecom Inc</summary>
    WAVE_FORMAT_SIPROLAB_G729A = 0x0134,

    /// <summary>Sipro Lab Telecom Inc</summary>
    WAVE_FORMAT_SIPROLAB_KELVIN = 0x0135,

    /// <summary>Dictaphone Corporation</summary>
    WAVE_FORMAT_G726ADPCM = 0x0140,

    /// <summary>Qualcomm, Inc</summary>
    WAVE_FORMAT_QUALCOMM_PUREVOICE = 0x0150,

    /// <summary>Qualcomm, Inc</summary>
    WAVE_FORMAT_QUALCOMM_HALFRATE = 0x0151,

    /// <summary>Ring Zero Systems, Inc</summary>
    WAVE_FORMAT_TUBGSM = 0x0155,

    /// <summary>Microsoft Corporation</summary>
    WAVE_FORMAT_MSAUDIO1 = 0x0160,

    /// <summary>Windows Media Audio, WAVE_FORMAT_WMAUDIO2, Microsoft Corporation</summary>
    WindowsMediaAudio = 0x0161,

    /// <summary>Windows Media Audio Professional WAVE_FORMAT_WMAUDIO3, Microsoft Corporation</summary>
    WindowsMediaAudioProfessional = 0x0162,

    /// <summary>Windows Media Audio Lossless, WAVE_FORMAT_WMAUDIO_LOSSLESS</summary>
    WindowsMediaAudioLosseless = 0x0163,

    /// <summary>Windows Media Audio Professional over SPDIF WAVE_FORMAT_WMASPDIF (0x0164)</summary>
    WindowsMediaAudioSpdif = 0x0164,

    /// <summary>Unisys Corp</summary>
    WAVE_FORMAT_UNISYS_NAP_ADPCM = 0x0170,

    /// <summary>Unisys Corp</summary>
    WAVE_FORMAT_UNISYS_NAP_ULAW = 0x0171,

    /// <summary>Unisys Corp</summary>
    WAVE_FORMAT_UNISYS_NAP_ALAW = 0x0172,

    /// <summary>Unisys Corp</summary>
    WAVE_FORMAT_UNISYS_NAP_16K = 0x0173,

    /// <summary>Creative Labs, Inc</summary>
    WAVE_FORMAT_CREATIVE_ADPCM = 0x0200,

    /// <summary>Creative Labs, Inc</summary>
    WAVE_FORMAT_CREATIVE_FASTSPEECH8 = 0x0202,

    /// <summary>Creative Labs, Inc</summary>
    WAVE_FORMAT_CREATIVE_FASTSPEECH10 = 0x0203,

    /// <summary>UHER informatic GmbH</summary>
    WAVE_FORMAT_UHER_ADPCM = 0x0210,

    /// <summary>Quarterdeck Corporation</summary>
    WAVE_FORMAT_QUARTERDECK = 0x0220,

    /// <summary>I-link Worldwide</summary>
    WAVE_FORMAT_ILINK_VC = 0x0230,

    /// <summary>Aureal Semiconductor</summary>
    WAVE_FORMAT_RAW_SPORT = 0x0240,

    /// <summary>ESS Technology, Inc</summary>
    WAVE_FORMAT_ESST_AC3 = 0x0241,

    /// <summary>Interactive Products, Inc</summary>
    WAVE_FORMAT_IPI_HSX = 0x0250,

    /// <summary>Interactive Products, Inc</summary>
    WAVE_FORMAT_IPI_RPELP = 0x0251,

    /// <summary>Consistent Software</summary>
    WAVE_FORMAT_CS2 = 0x0260,

    /// <summary>Sony Corp</summary>
    WAVE_FORMAT_SONY_SCX = 0x0270,

    /// <summary>Fujitsu Corp</summary>
    WAVE_FORMAT_FM_TOWNS_SND = 0x0300,

    /// <summary>Brooktree Corporation</summary>
    WAVE_FORMAT_BTV_DIGITAL = 0x0400,

    /// <summary>QDesign Corporation</summary>
    WAVE_FORMAT_QDESIGN_MUSIC = 0x0450,

    /// <summary>AT&T Labs, Inc</summary>
    WAVE_FORMAT_VME_VMPCM = 0x0680,

    /// <summary>AT&T Labs, Inc</summary>
    WAVE_FORMAT_TPC = 0x0681,

    /// <summary>Ing C. Olivetti & C., S.p.A.</summary>
    WAVE_FORMAT_OLIGSM = 0x1000,

    /// <summary>Ing C. Olivetti & C., S.p.A.</summary>
    WAVE_FORMAT_OLIADPCM = 0x1001,

    /// <summary>Ing C. Olivetti & C., S.p.A.</summary>
    WAVE_FORMAT_OLICELP = 0x1002,

    /// <summary>Ing C. Olivetti & C., S.p.A.</summary>
    WAVE_FORMAT_OLISBC = 0x1003,

    /// <summary>Ing C. Olivetti & C., S.p.A.</summary>
    WAVE_FORMAT_OLIOPR = 0x1004,

    /// <summary>Lernout & Hauspie</summary>
    WAVE_FORMAT_LH_CODEC = 0x1100,

    /// <summary>Norris Communications, Inc</summary>
    WAVE_FORMAT_NORRIS = 0x1400,

    /// <summary>AT&T Labs, Inc</summary>
    WAVE_FORMAT_SOUNDSPACE_MUSICOMPRESS = 0x1500,

    /// <summary>
    /// Advanced Audio Coding (AAC) audio in Audio Data Transport Stream (ADTS) format.
    /// The format block is a WAVEFORMATEX structure with wFormatTag equal to WAVE_FORMAT_MPEG_ADTS_AAC.
    /// </summary>
    /// <remarks>
    /// The WAVEFORMATEX structure specifies the core AAC-LC sample rate and number of channels, 
    /// prior to applying spectral band replication (SBR) or parametric stereo (PS) tools, if present.
    /// No additional data is required after the WAVEFORMATEX structure.
    /// </remarks>
    /// <see>http://msdn.microsoft.com/en-us/library/dd317599%28VS.85%29.aspx</see>
    MPEG_ADTS_AAC = 0x1600,

    /// <remarks>Source wmCodec.h</remarks>
    MPEG_RAW_AAC = 0x1601,

    /// <summary>
    /// MPEG-4 audio transport stream with a synchronization layer (LOAS) and a multiplex layer (LATM).
    /// The format block is a WAVEFORMATEX structure with wFormatTag equal to WAVE_FORMAT_MPEG_LOAS.
    /// </summary>
    /// <remarks>
    /// The WAVEFORMATEX structure specifies the core AAC-LC sample rate and number of channels, 
    /// prior to applying spectral SBR or PS tools, if present.
    /// No additional data is required after the WAVEFORMATEX structure.
    /// </remarks>
    /// <see>http://msdn.microsoft.com/en-us/library/dd317599%28VS.85%29.aspx</see>
    MPEG_LOAS = 0x1602,

    /// <summary>NOKIA_MPEG_ADTS_AAC</summary>
    /// <remarks>Source wmCodec.h</remarks>
    NOKIA_MPEG_ADTS_AAC = 0x1608,

    /// <summary>NOKIA_MPEG_RAW_AAC</summary>
    /// <remarks>Source wmCodec.h</remarks>
    NOKIA_MPEG_RAW_AAC = 0x1609,

    /// <summary>VODAFONE_MPEG_ADTS_AAC</summary>
    /// <remarks>Source wmCodec.h</remarks>
    VODAFONE_MPEG_ADTS_AAC = 0x160A,

    /// <summary>VODAFONE_MPEG_RAW_AAC</summary>
    /// <remarks>Source wmCodec.h</remarks>
    VODAFONE_MPEG_RAW_AAC = 0x160B,

    /// <summary>
    /// High-Efficiency Advanced Audio Coding (HE-AAC) stream.
    /// The format block is an HEAACWAVEFORMAT structure.
    /// </summary>
    /// <see>http://msdn.microsoft.com/en-us/library/dd317599%28VS.85%29.aspx</see>
    MPEG_HEAAC = 0x1610,

    /// <summary>WAVE_FORMAT_DVM</summary>
    WAVE_FORMAT_DVM = 0x2000, // FAST Multimedia AG

    // others - not from MS headers
    /// <summary>WAVE_FORMAT_VORBIS1 "Og" Original stream compatible</summary>
    Vorbis1 = 0x674f,

    /// <summary>WAVE_FORMAT_VORBIS2 "Pg" Have independent header</summary>
    Vorbis2 = 0x6750,

    /// <summary>WAVE_FORMAT_VORBIS3 "Qg" Have no codebook header</summary>
    Vorbis3 = 0x6751,

    /// <summary>WAVE_FORMAT_VORBIS1P "og" Original stream compatible</summary>
    Vorbis1P = 0x676f,

    /// <summary>WAVE_FORMAT_VORBIS2P "pg" Have independent headere</summary>
    Vorbis2P = 0x6770,

    /// <summary>WAVE_FORMAT_VORBIS3P "qg" Have no codebook header</summary>
    Vorbis3P = 0x6771,

    /// <summary>WAVE_FORMAT_EXTENSIBLE</summary>
    Extensible = 0xFFFE, // Microsoft

    /// <summary></summary>
    WAVE_FORMAT_DEVELOPMENT = 0xFFFF,
}
