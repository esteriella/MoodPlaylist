import { TailSpin } from 'react-loader-spinner';

function Loader() {
  return <TailSpin color='#f6339a'height={80} width={80} ariaLabel="loading" />;  
}

export default Loader;