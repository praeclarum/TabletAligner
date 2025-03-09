function createAlignmentEditor(canvasId, pubId) {

const $canvas = document.getElementById(canvasId);
const ctx = canvas.getContext("2d");
const canvasWidth = 1000;
const canvasHeight = 1000;
canvas.width = canvasWidth;
canvas.height = canvasHeight;

var img = new Image();
var imgScale = 1;
var textAreaBounds = [{
    name: "obverse",
    x: 0.25,
    y: 0.1,
    width: 0.5,
    height: 0.5
}];
const boundaryLineThickness = 11;
const boundaryHandleRadius = 22;
var mouseIsDown = false;

function getTextAreaCanvasBounds(areaIndex) {
    const bounds = textAreaBounds[areaIndex];
    const x = bounds.x * img.width * imgScale;
    const y = bounds.y * img.height * imgScale;
    const width = bounds.width * img.width * imgScale;
    const height = bounds.height * img.height * imgScale;
    return {
        x: x,
        y: y,
        width: width,
        height: height
    };
}

function hitTestTextAreaHandle(x, y) {
    for (var i = 0; i < textAreaBounds.length; i++) {
        const bounds = textAreaBounds[i];
        const a = getTextAreaCanvasBounds(i);
        const ax = a.x;
        const ay = a.y;
        const awidth = a.width;
        const aheight = a.height;
        if (ax - boundaryHandleRadius <= x && x <= ax + boundaryHandleRadius && ay - boundaryHandleRadius <= y && y <= ay + boundaryHandleRadius) {
            return {
                handle: "topLeft",
                areaIndex: i
            };
        }
        if (ax - boundaryHandleRadius <= x && x <= ax + boundaryHandleRadius && ay + aheight - boundaryHandleRadius <= y && y <= ay + aheight + boundaryHandleRadius) {
            return {
                handle: "bottomLeft",
                areaIndex: i
            };
        }
        if (ax + awidth - boundaryHandleRadius <= x && x <= ax + awidth + boundaryHandleRadius && ay - boundaryHandleRadius <= y && y <= ay + boundaryHandleRadius) {
            return {
                handle: "topRight",
                areaIndex: i
            };
        }
        if (ax + awidth - boundaryHandleRadius <= x && x <= ax + awidth + boundaryHandleRadius && ay + aheight - boundaryHandleRadius <= y && y <= ay + aheight + boundaryHandleRadius) {
            return {
                handle: "bottomRight",
                areaIndex: i
            };
        }
    }
    return null;
}

var mouseIsDown = false;
var mouseDownHitTestResult = null;
var mouseLastX = 0;
var mouseLastY = 0;

$canvas.onmousedown = function(e){
    const x = e.offsetX;
    const y = e.offsetY;
    const hitTestResult = hitTestTextAreaHandle(x, y);
    if (hitTestResult) {
        console.log("hit handle", x, y, hitTestResult);
        mouseIsDown = true;
        mouseDownHitTestResult = hitTestResult;
        mouseLastX = x;
        mouseLastY = y;
        draw();
    }
}
$canvas.onmouseup = function(e){
    if (mouseIsDown) {
        const x = e.offsetX;
        const y = e.offsetY;
        console.log("mouseup", x, y);
        mouseIsDown = false;
        mouseDownHitTestResult = null;
        draw();
    }
}
$canvas.onmousemove = function(e){
    if (!mouseIsDown) return;
    const normalizedImageToCanvasScaleX = img.width * imgScale;
    const canvasToNormalizedImageScaleX = 1.0 / normalizedImageToCanvasScaleX;
    const normalizedImageToCanvasScaleY = img.height * imgScale;
    const canvasToNormalizedImageScaleY = 1.0 / normalizedImageToCanvasScaleY;
    const x = e.offsetX;
    const y = e.offsetY;
    const dx = x - mouseLastX;
    const dy = y - mouseLastY;
    mouseLastX = x;
    mouseLastY = y;
    if (normalizedImageToCanvasScaleX > 0 && normalizedImageToCanvasScaleY > 0) {
        const area = textAreaBounds[mouseDownHitTestResult.areaIndex];
        const a = getTextAreaCanvasBounds(mouseDownHitTestResult.areaIndex);
        if (mouseDownHitTestResult.handle == "topLeft") {
            area.x = (a.x + dx) * canvasToNormalizedImageScaleX;
            area.y = (a.y + dy) * canvasToNormalizedImageScaleY;
            area.width = (a.width - dx) * canvasToNormalizedImageScaleX;
            area.height = (a.height - dy) * canvasToNormalizedImageScaleY;
        }
        if (mouseDownHitTestResult.handle == "bottomLeft") {
            area.x = (a.x + dx) * canvasToNormalizedImageScaleX;
            area.y = (a.y) * canvasToNormalizedImageScaleY;
            area.width = (a.width - dx) * canvasToNormalizedImageScaleX;
            area.height = (a.height + dy) * canvasToNormalizedImageScaleY;
        }
        if (mouseDownHitTestResult.handle == "topRight") {
            area.x = (a.x) * canvasToNormalizedImageScaleX;
            area.y = (a.y + dy) * canvasToNormalizedImageScaleY;
            area.width = (a.width + dx) * canvasToNormalizedImageScaleX;
            area.height = (a.height - dy) * canvasToNormalizedImageScaleY;
        }
        if (mouseDownHitTestResult.handle == "bottomRight") {
            area.x = (a.x) * canvasToNormalizedImageScaleX;
            area.y = (a.y) * canvasToNormalizedImageScaleY;
            area.width = (a.width + dx) * canvasToNormalizedImageScaleX;
            area.height = (a.height + dy) * canvasToNormalizedImageScaleY;
        }
        area.width = Math.max(area.width, 0.01);
        area.height = Math.max(area.height, 0.01);
        if (area.x + area.width > 1) {
            area.x = 1 - area.width;
        }
        if (area.y + area.height > 1) {
            area.y = 1 - area.height;
        }
        if (area.x < 0) {
            area.x = 0;
        }
        if (area.y < 0) {
            area.y = 0;
        }
        draw();
    }
    return false;
}
function draw() {
    // Draw checkerboard background
    ctx.fillStyle = "#aaa";
    ctx.fillRect(0, 0, canvasWidth, canvasHeight);
    ctx.fillStyle = "#888";
    for (var i = 0; i < canvasWidth; i += 50) {
        for (var j = 0; j < canvasHeight; j += 50) {
            if ((i + j) % 100 == 0) {
                ctx.fillRect(i, j, 50, 50);
            }
        }
    }
    // Draw image
    // console.log(img.width, img.height);
    const xscale = canvasWidth / img.width;
    const yscale = canvasHeight / img.height;
    imgScale = Math.min(xscale, yscale);
    ctx.drawImage(img, 0, 0, img.width * imgScale, img.height * imgScale);
    // Draw text area boundaries
    ctx.fillStyle = "rgba(64,64,255,0.8)";
    ctx.font = "16px Arial";
    ctx.textBaseline = "middle";
    for (var i = 0; i < textAreaBounds.length; i++) {
        const area = textAreaBounds[i];
        const a = getTextAreaCanvasBounds(i);
        const x = a.x, y = a.y, width = a.width, height = a.height;
        ctx.strokeStyle = "rgba(64,64,255,0.8)";
        ctx.lineWidth = boundaryLineThickness;
        ctx.save();
        // clip away the inner rectangle
        ctx.beginPath();
        ctx.rect(0, 0, x, canvasHeight);
        ctx.rect(x + width, 0, canvasWidth - x - width, canvasHeight);
        ctx.rect(0, 0, canvasWidth, y);
        ctx.rect(0, y + height, canvasWidth, canvasHeight - y - height);
        ctx.clip();
        // Draw boundary lines
        ctx.strokeRect(x, y, width, height);
        // Draw boundary drag handles
        ctx.beginPath();
        ctx.arc(x, y, boundaryHandleRadius, 0, 2 * Math.PI);
        ctx.fill();
        ctx.beginPath();
        ctx.arc(x + width, y, boundaryHandleRadius, 0, 2 * Math.PI);
        ctx.fill();
        ctx.beginPath();
        ctx.arc(x + width, y + height, boundaryHandleRadius, 0, 2 * Math.PI);
        ctx.fill();
        ctx.beginPath();
        ctx.arc(x, y + height, boundaryHandleRadius, 0, 2 * Math.PI);
        ctx.fill();
        // Draw text area name centered on the top edge of the area with a solid background
        const name = area.name;
        const nameWidth = ctx.measureText(name).width;
        ctx.fillStyle = "rgba(64,64,255,0.8)";
        ctx.fillRect(x + width / 2 - nameWidth / 2, y - 20, nameWidth + 10, 20);
        ctx.fillStyle = "white";
        ctx.fillText(name, x + width / 2 - nameWidth / 2 + 5, y - 10);
        ctx.restore();
        // Draw thin lines for the rows
        ctx.strokeStyle = "rgba(64,64,255,0.8)";
        ctx.lineWidth = 2;
        for (let row=0; row<area.numRows; row++) {
            const rowHeight = height / area.numRows;
            const ry = y + (row + 1) * rowHeight;
            const text = area.rows[row];
            const textSize = ctx.measureText(text);
            const textScaleX = width / textSize.width;
            const textScaleY = height / area.numRows / textSize.height;
            const textScale = Math.min(textScaleX, textScaleY);
            ctx.save();
            ctx.fillStyle = "rgba(64,64,255,0.5)";
            ctx.scale(textScale, textScale);
            ctx.fillText(text, x + width / 2 - textSize.width / 2, ry - rowHeight/2);
            ctx.restore();
            ctx.beginPath();
            ctx.moveTo(x, ry);
            ctx.lineTo(x + width, ry);
            ctx.stroke();
        }
    }
}
img.src = `https://cdli.mpiwg-berlin.mpg.de/dl/photo/${pubId}.jpg`;
img.onload = function() {
    draw();
};
img.onerror = function() {
    console.log("Error loading image");
    draw();
};
return {
    setTextAreaBounds: function(bounds) {
        textAreaBounds = bounds;
        draw();
    },
    getTextAreaBounds: function() {
        return textAreaBounds;
    }
};
}
