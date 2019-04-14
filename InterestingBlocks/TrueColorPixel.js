x+=o.X;z+=o.Y;
w+=o.X;h+=o.Y;
;x<w;x++
;z<h;z++
[x/r,z/r][x%r,z%r]=img.GetPixel(x-o.x,z-o.y)