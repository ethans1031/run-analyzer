import { NavLink } from "react-router-dom";
import "./NavBar.css";

function NavBar() {
    return (
        <nav className="navbar">
            <div className="navbar-inner container">
                <div className="navbar-brand">Spire Tracker</div>
                <div className="navbar-links">
                    <NavLink 
                        to="/" 
                        end
                        className={({ isActive }) => 
                            "navbar-link" + (isActive ? " navbar-link--active" : "")
                        }
                    >
                        Runs
                    </NavLink>
                    <NavLink 
                        to="/stats"
                        className={({ isActive }) => 
                            "navbar-link" + (isActive ? " navbar-link--active" : "")
                        }
                    >
                        Stats
                    </NavLink>
                    <NavLink 
                        to="/upload"
                        className={({ isActive }) => 
                            "navbar-link" + (isActive ? " navbar-link--active" : "")
                        }
                    >
                        Upload
                    </NavLink>
                </div>
            </div>
        </nav>
    );
}

export default NavBar;