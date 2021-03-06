import * as React from 'react';
import './UploadButton.css';
import '../CircleButton.css';

const uploadButton = (props: any) => {
        return (
            <button className="circle-btn upload-btn" onClick={props.click} disabled={props.disabled}/>
        );
}

export default uploadButton;