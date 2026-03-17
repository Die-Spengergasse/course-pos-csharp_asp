convert_adoc asp_book.asciidoc asp_book_print.pdf ^
&& C:\ghostscript\bin\gswin64c.exe -q -sDEVICE=pdfwrite ^
-dPDFSETTINGS=/printer -dColorImageResolution=150 -dNOPAUSE ^
-dBATCH -sOutputFile=asp_book.pdf asp_book_print.pdf
