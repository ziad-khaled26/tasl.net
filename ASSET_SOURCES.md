# Runtime assets and demo content

## Falcon 3.24.0

Source: the user-supplied `Reference/falcon-v3.24.0.zip`, extracted to `Reference/Falcon`.

Inspected references:

- `public/widgets.html`
- `public/dashboard/e-commerce.html`
- `public/app/e-commerce/product/product-grid.html`
- `public/app/e-commerce/product/product-details.html`
- `public/modules/forms/basic/form-control.html`
- `public/modules/tables/basic-tables.html`
- `public/app/e-commerce/orders/order-list.html`
- `public/app/e-commerce/invoice.html`

Runtime copies: compiled theme CSS, configuration and theme JavaScript, Bootstrap, Popper, AnchorJS, is.js, Lodash, Font Awesome, and two images referenced by the compiled CSS. Files are copied from the supplied archive; third-party headers and license notices are preserved. The commercial Falcon template remains subject to its original purchaser's license. This project does not grant a new license to redistribute the template archive.

Bootstrap and Popper use MIT licenses, Lodash and AnchorJS use MIT licenses, is.js uses an MIT license. The bundled Font Awesome distribution includes its license header (icons CC BY 4.0, code MIT, fonts SIL OFL where applicable). See the notices in the supplied files. The existing local jQuery validation distributions retain their original license files.

## Local fonts

The reference HTML requests Open Sans and Poppins through Google Fonts. No `.ttf`, `.woff` or `.woff2` font files were present in the supplied archive. The following upstream font files were downloaded once and included locally; runtime does not contact Google:

- [Open Sans variable font](https://github.com/google/fonts/tree/main/ofl/opensans): `OpenSans[wdth,wght].ttf`, stored as `wwwroot/fonts/OpenSans.ttf`.
- [Poppins](https://github.com/google/fonts/tree/main/ofl/poppins): `Poppins-Regular.ttf` and `Poppins-SemiBold.ttf`.

Both distributions are covered by the SIL Open Font License supplied alongside them in `wwwroot/fonts/OpenSans-LICENSE.txt` and `Poppins-LICENSE.txt`. Preserve those notices when distributing the fonts. The font names and upstream copyright notices are unchanged.

## Demo raster illustrations

`wwwroot/images/movie-1.png` through `movie-6.png`, `cinema-1.png` through `cinema-3.png`, and `actor-1.png` through `actor-8.png` are original procedural illustrations created locally for this project by `tools/create_demo_art.py`. They are clearly labelled placeholders. None depicts an actual performer, released film poster, or real cinema photograph. All sample films, performer names and cinema brands are fictional educational content.

These original demo graphics may be used, modified and replaced with this educational project without an external image subscription or attribution requirement. They are not represented as licensed movie publicity material. Text was rasterized using the locally installed Windows Segoe UI font; no Segoe UI font file is distributed.

To replace a graphic, upload an image from the corresponding administration form. Use your own image or one with permission covering your intended use, and record its source and license here. Do not present the actor placeholders as photographs of real actors.

Optional regeneration on Windows:

```powershell
python -m pip install Pillow==13.0.0
python tools/create_demo_art.py
```

The generated PNG files are already supplied, so Python and Pillow are not application prerequisites.

## Image upload library

[SkiaSharp](https://github.com/mono/SkiaSharp) is an MIT-licensed image processing dependency distributed by NuGet together with its native dependencies and notices. It decodes accepted raster uploads and writes a new JPEG with generated filename. It does not require a subscription or API key. Package licenses and notices remain applicable when distributing application binaries.
